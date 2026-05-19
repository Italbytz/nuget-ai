import * as d3 from "https://cdn.jsdelivr.net/npm/d3@7/+esm";

const FEATURE_LABELS = {
  sepal_length: "Sepal Length",
  sepal_width: "Sepal Width",
  petal_length: "Petal Length",
  petal_width: "Petal Width"
};

const COLORS = ["#2563eb", "#16a34a", "#ea580c", "#7c3aed", "#0891b2", "#be123c"];
const SYMBOLS = [d3.symbolCircle, d3.symbolTriangle, d3.symbolSquare, d3.symbolDiamond, d3.symbolWye, d3.symbolStar];
const controllers = new WeakMap();

export function renderIrisClustering(host, model) {
  if (!host || !model || !Array.isArray(model.points)) {
    return;
  }

  const xKey = model.xAxis;
  const yKey = model.yAxis;

  if (!xKey || !yKey || xKey === yKey) {
    host.innerHTML = "<div style='padding:1rem;color:#64748b'>Please choose two different feature axes.</div>";
    return;
  }

  const width = Math.max(host.clientWidth || 760, 420);
  const height = Math.max(host.clientHeight || 430, 320);
  const margin = { top: 30, right: 28, bottom: 58, left: 66 };

  const points = model.points.filter((p) => Number.isFinite(p[xKey]) && Number.isFinite(p[yKey]));
  const centroids = Array.isArray(model.centroids)
    ? model.centroids.filter((c) => Number.isFinite(c[xKey]) && Number.isFinite(c[yKey]))
    : [];
  const centroidFrames = Array.isArray(model.centroidFrames)
    ? model.centroidFrames
        .map((frame) => ({
          iteration: frame.iteration,
          centroids: Array.isArray(frame.centroids)
            ? frame.centroids.filter((c) => Number.isFinite(c[xKey]) && Number.isFinite(c[yKey]))
            : []
        }))
        .filter((frame) => frame.centroids.length > 0)
    : [];

  if (points.length === 0) {
    host.innerHTML = "<div style='padding:1rem;color:#64748b'>No numeric iris data available for this projection.</div>";
    return;
  }

  host.innerHTML = "";
  const runToken = (host.__irisRunToken || 0) + 1;
  host.__irisRunToken = runToken;
  const previousController = controllers.get(host);
  if (previousController?.timerId) {
    clearTimeout(previousController.timerId);
  }

  const clusterIds = Array.from(new Set(points.map((p) => p.cluster))).sort((a, b) => a - b);
  const color = d3.scaleOrdinal(clusterIds, clusterIds.map((_, i) => COLORS[i % COLORS.length]));
  const speciesIds = Array.from(new Set(points.map((p) => p.species))).sort((a, b) => a.localeCompare(b));
  const symbolType = d3.scaleOrdinal(speciesIds, speciesIds.map((_, i) => SYMBOLS[i % SYMBOLS.length]));

  const xExtent = d3.extent(points, (d) => d[xKey]);
  const yExtent = d3.extent(points, (d) => d[yKey]);
  const xPad = (xExtent[1] - xExtent[0]) * 0.08 || 0.5;
  const yPad = (yExtent[1] - yExtent[0]) * 0.08 || 0.5;

  const xScale = d3
    .scaleLinear()
    .domain([xExtent[0] - xPad, xExtent[1] + xPad])
    .range([margin.left, width - margin.right]);

  const yScale = d3
    .scaleLinear()
    .domain([yExtent[0] - yPad, yExtent[1] + yPad])
    .range([height - margin.bottom, margin.top]);

  const svg = d3
    .select(host)
    .append("svg")
    .attr("width", width)
    .attr("height", height)
    .attr("viewBox", `0 0 ${width} ${height}`)
    .attr("role", "img")
    .attr("aria-label", `Iris clustering plot for ${FEATURE_LABELS[xKey]} vs ${FEATURE_LABELS[yKey]}`);

  svg
    .append("rect")
    .attr("x", margin.left)
    .attr("y", margin.top)
    .attr("width", width - margin.left - margin.right)
    .attr("height", height - margin.top - margin.bottom)
    .attr("fill", "rgba(255,255,255,0.65)");

  const xGrid = d3.axisBottom(xScale).ticks(8).tickSize(-(height - margin.top - margin.bottom)).tickFormat("");
  const yGrid = d3.axisLeft(yScale).ticks(8).tickSize(-(width - margin.left - margin.right)).tickFormat("");

  svg
    .append("g")
    .attr("transform", `translate(0, ${height - margin.bottom})`)
    .call(xGrid)
    .selectAll("line")
    .attr("stroke", "#cbd5e1")
    .attr("stroke-dasharray", "2,3");

  svg
    .append("g")
    .attr("transform", `translate(${margin.left}, 0)`)
    .call(yGrid)
    .selectAll("line")
    .attr("stroke", "#cbd5e1")
    .attr("stroke-dasharray", "2,3");

  const xAxis = d3.axisBottom(xScale).ticks(8);
  const yAxis = d3.axisLeft(yScale).ticks(8);

  svg
    .append("g")
    .attr("transform", `translate(0, ${height - margin.bottom})`)
    .call(xAxis)
    .call((g) => g.selectAll("text").attr("fill", "#334155"))
    .call((g) => g.selectAll("path,line").attr("stroke", "#64748b"));

  svg
    .append("g")
    .attr("transform", `translate(${margin.left}, 0)`)
    .call(yAxis)
    .call((g) => g.selectAll("text").attr("fill", "#334155"))
    .call((g) => g.selectAll("path,line").attr("stroke", "#64748b"));

  svg
    .append("text")
    .attr("x", (margin.left + width - margin.right) / 2)
    .attr("y", height - 16)
    .attr("text-anchor", "middle")
    .attr("fill", "#0f172a")
    .attr("font-size", 13)
    .attr("font-weight", 600)
    .text(FEATURE_LABELS[xKey] || xKey);

  svg
    .append("text")
    .attr("transform", `translate(18, ${(margin.top + height - margin.bottom) / 2}) rotate(-90)`)
    .attr("text-anchor", "middle")
    .attr("fill", "#0f172a")
    .attr("font-size", 13)
    .attr("font-weight", 600)
    .text(FEATURE_LABELS[yKey] || yKey);

  const dots = svg.append("g").attr("aria-label", "data points");
  const symbolBuilder = d3.symbol().size(76);

  dots
    .selectAll("path")
    .data(points, (d) => d.index)
    .join("path")
    .attr("transform", (d) => `translate(${xScale(d[xKey])},${yScale(d[yKey])}) scale(0.15)`)
    .attr("d", (d) => symbolBuilder.type(symbolType(d.species))())
    .attr("fill", (d) => color(d.cluster))
    .attr("fill-opacity", 0.75)
    .attr("stroke", "#0f172a")
    .attr("stroke-width", 0.65)
    .append("title")
    .text((d) => `#${d.index} | ${d.species} | Cluster ${d.cluster}`);

  dots
    .selectAll("path")
    .transition()
    .duration(380)
    .ease(d3.easeCubicOut)
    .attr("transform", (d) => `translate(${xScale(d[xKey])},${yScale(d[yKey])}) scale(1)`);

  const centroidGroup = svg.append("g").attr("aria-label", "centroids");

  centroidGroup
    .selectAll("path")
    .data(centroids, (d) => d.cluster)
    .join("path")
    .attr("transform", (d) => `translate(${xScale(d[xKey])},${yScale(d[yKey])})`)
    .attr("d", d3.symbol().type(d3.symbolCross).size(130))
    .attr("fill", "none")
    .attr("stroke", (d) => color(d.cluster))
    .attr("stroke-width", 2.4)
    .append("title")
    .text((d) => `Centroid ${d.cluster}`);

  const stepBadge = svg
    .append("text")
    .attr("x", margin.left + 8)
    .attr("y", margin.top + 16)
    .attr("fill", "#0f172a")
    .attr("font-size", 12)
    .attr("font-weight", 700)
    .text("Iteration 0");

  const framesToAnimate = centroidFrames.length > 0 ? centroidFrames : [{ iteration: 0, centroids }];

  const controller = {
    paused: false,
    timerId: null,
    frameIndex: 0,
    intervalMs: 560,
    play,
    pause,
    replay,
    step
  };
  controllers.set(host, controller);
  play();

  const legend = svg
    .append("g")
    .attr("transform", `translate(${width - margin.right - 120}, ${margin.top + 6})`);

  clusterIds.forEach((clusterId, idx) => {
    const row = legend.append("g").attr("transform", `translate(0, ${idx * 18})`);

    row
      .append("circle")
      .attr("r", 5)
      .attr("cx", 0)
      .attr("cy", 0)
      .attr("fill", color(clusterId));

    row
      .append("text")
      .attr("x", 10)
      .attr("y", 4)
      .attr("fill", "#0f172a")
      .attr("font-size", 12)
      .text(`Cluster ${clusterId}`);
  });

  const speciesLegend = svg
    .append("g")
    .attr("transform", `translate(${margin.left + 6}, ${height - margin.bottom - Math.max(12, speciesIds.length * 18)})`);

  speciesIds.forEach((species, idx) => {
    const row = speciesLegend.append("g").attr("transform", `translate(0, ${idx * 18})`);

    row
      .append("path")
      .attr("d", symbolBuilder.type(symbolType(species))())
      .attr("transform", "translate(0,0)")
      .attr("fill", "#475569")
      .attr("stroke", "#1e293b")
      .attr("stroke-width", 0.7);

    row
      .append("text")
      .attr("x", 12)
      .attr("y", 4)
      .attr("fill", "#0f172a")
      .attr("font-size", 12)
      .text(species);
  });

  function drawFrame(frameIndex) {
    const frame = framesToAnimate[Math.max(0, Math.min(frameIndex, framesToAnimate.length - 1))];
    stepBadge.text(`Iteration ${frame.iteration}`);

    centroidGroup
      .selectAll("path")
      .data(frame.centroids, (d) => d.cluster)
      .join("path")
      .attr("d", d3.symbol().type(d3.symbolCross).size(130))
      .attr("fill", "none")
      .attr("stroke", (d) => color(d.cluster))
      .attr("stroke-width", 2.4)
      .transition()
      .duration(520)
      .ease(d3.easeCubicInOut)
      .attr("transform", (d) => `translate(${xScale(d[xKey])},${yScale(d[yKey])})`);
  }

  function scheduleNextFrame() {
    if (host.__irisRunToken !== runToken || controller.paused) {
      return;
    }

    if (controller.frameIndex >= framesToAnimate.length) {
      controller.timerId = null;
      return;
    }

    drawFrame(controller.frameIndex);
    controller.frameIndex += 1;

    if (controller.frameIndex < framesToAnimate.length) {
      controller.timerId = setTimeout(scheduleNextFrame, controller.intervalMs);
    } else {
      controller.timerId = null;
    }
  }

  function play() {
    if (host.__irisRunToken !== runToken) {
      return;
    }

    controller.paused = false;
    if (controller.timerId) {
      return;
    }

    scheduleNextFrame();
  }

  function pause() {
    controller.paused = true;
    if (controller.timerId) {
      clearTimeout(controller.timerId);
      controller.timerId = null;
    }
  }

  function replay() {
    pause();
    controller.frameIndex = 0;
    drawFrame(0);
    controller.frameIndex = 1;
    play();
  }

  function step() {
    pause();

    if (controller.frameIndex >= framesToAnimate.length) {
      controller.frameIndex = 0;
    }

    drawFrame(controller.frameIndex);
    controller.frameIndex += 1;
  }
}

export function playIrisClustering(host) {
  controllers.get(host)?.play();
}

export function pauseIrisClustering(host) {
  controllers.get(host)?.pause();
}

export function replayIrisClustering(host) {
  controllers.get(host)?.replay();
}

export function stepIrisClustering(host) {
  controllers.get(host)?.step();
}
