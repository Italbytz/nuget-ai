import * as d3 from "https://cdn.jsdelivr.net/npm/d3@7/+esm";

const controllers = new WeakMap();

export function renderTaxiRegressionD3(host, model) {
  if (!host || !model || !Array.isArray(model.points)) {
    return;
  }

  const points = model.points;
  if (points.length === 0) {
    host.innerHTML = "";
    return;
  }

  const width = Math.max(host.clientWidth || 760, 420);
  const height = Math.max(host.clientHeight || 340, 260);
  host.innerHTML = "";
  const previous = controllers.get(host);
  if (previous?.timerId) {
    clearTimeout(previous.timerId);
  }

  const svg = d3.select(host).append("svg").attr("width", width).attr("height", height).attr("viewBox", `0 0 ${width} ${height}`);

  const margin = { top: 26, right: 20, bottom: 46, left: 52 };
  const leftWidth = Math.round(width * 0.62);
  const chartW = leftWidth - margin.left - margin.right;
  const chartH = height - margin.top - margin.bottom;

  const maxFare = d3.max(points, (d) => Math.max(d.actual, d.predicted)) ?? 1;
  const x = d3.scaleLinear().domain([0, maxFare * 1.08]).range([margin.left, margin.left + chartW]);
  const y = d3.scaleLinear().domain([0, maxFare * 1.08]).range([height - margin.bottom, margin.top]);

  svg
    .append("line")
    .attr("x1", x(0))
    .attr("y1", y(0))
    .attr("x2", x(maxFare * 1.08))
    .attr("y2", y(maxFare * 1.08))
    .attr("stroke", "#94a3b8")
    .attr("stroke-dasharray", "4,4");

  svg
    .append("line")
    .attr("x1", x(0))
    .attr("y1", y(2))
    .attr("x2", x(maxFare * 1.08))
    .attr("y2", y(maxFare * 1.08 + 2))
    .attr("stroke", "#cbd5e1")
    .attr("stroke-dasharray", "2,4");

  svg
    .append("line")
    .attr("x1", x(0))
    .attr("y1", y(-2))
    .attr("x2", x(maxFare * 1.08))
    .attr("y2", y(maxFare * 1.08 - 2))
    .attr("stroke", "#cbd5e1")
    .attr("stroke-dasharray", "2,4");

  svg
    .append("g")
    .attr("transform", `translate(0, ${height - margin.bottom})`)
    .call(d3.axisBottom(x).ticks(6))
    .call((g) => g.selectAll("text").attr("fill", "#334155"));

  svg
    .append("g")
    .attr("transform", `translate(${margin.left}, 0)`)
    .call(d3.axisLeft(y).ticks(6))
    .call((g) => g.selectAll("text").attr("fill", "#334155"));

  svg
    .append("text")
    .attr("x", margin.left + chartW / 2)
    .attr("y", height - 12)
    .attr("text-anchor", "middle")
    .attr("fill", "#0f172a")
    .attr("font-size", 12)
    .text("Actual fare ($)");

  svg
    .append("text")
    .attr("transform", `translate(14, ${margin.top + chartH / 2}) rotate(-90)`)
    .attr("text-anchor", "middle")
    .attr("fill", "#0f172a")
    .attr("font-size", 12)
    .text("Predicted fare ($)");

  svg
    .append("text")
    .attr("x", margin.left)
    .attr("y", 16)
    .attr("fill", "#0f172a")
    .attr("font-size", 12)
    .attr("font-weight", 700)
    .text("Predicted vs actual");

  const scatter = svg
    .append("g")
    .selectAll("circle")
    .data(points, (d) => d.index)
    .join("circle")
    .attr("cx", (d) => x(d.actual))
    .attr("cy", (d) => y(d.predicted))
    .attr("r", 4.6)
    .attr("fill", (d) => (d.withinTolerance ? "#16a34a" : "#ef4444"))
    .attr("fill-opacity", 0.82)
    .append("title")
    .text((d) => `#${d.index} actual=${d.actual.toFixed(2)} pred=${d.predicted.toFixed(2)}`);

  const focusRing = svg
    .append("circle")
    .attr("r", 8)
    .attr("fill", "none")
    .attr("stroke", "#0f172a")
    .attr("stroke-width", 1.8)
    .attr("stroke-dasharray", "4,3");

  const residuals = points.map((d) => ({ index: d.index, residual: d.predicted - d.actual, withinTolerance: d.withinTolerance }));
  const absMaxResidual = d3.max(residuals, (d) => Math.abs(d.residual)) ?? 1;

  const barX0 = leftWidth + 22;
  const barWidth = width - barX0 - 16;
  const barY = d3
    .scaleBand()
    .domain(residuals.map((d) => d.index))
    .range([margin.top, height - margin.bottom])
    .padding(0.2);
  const barX = d3.scaleLinear().domain([-absMaxResidual * 1.1, absMaxResidual * 1.1]).range([barX0, barX0 + barWidth]);

  svg
    .append("text")
    .attr("x", barX0)
    .attr("y", 16)
    .attr("fill", "#0f172a")
    .attr("font-size", 12)
    .attr("font-weight", 700)
    .text("Residuals (pred - actual)");

  svg
    .append("line")
    .attr("x1", barX(0))
    .attr("x2", barX(0))
    .attr("y1", margin.top)
    .attr("y2", height - margin.bottom)
    .attr("stroke", "#64748b")
    .attr("stroke-width", 1.2);

  svg
    .append("g")
    .selectAll("rect")
    .data(residuals)
    .join("rect")
    .attr("x", (d) => Math.min(barX(0), barX(d.residual)))
    .attr("y", (d) => barY(d.index) ?? 0)
    .attr("width", (d) => Math.abs(barX(d.residual) - barX(0)))
    .attr("height", barY.bandwidth())
    .attr("rx", 4)
    .attr("fill", (d) => (d.withinTolerance ? "#22c55e" : "#f97316"));

  const controller = {
    frameIndex: 0,
    timerId: null,
    paused: false,
    intervalMs: 620,
    play,
    pause,
    replay,
    step
  };
  controllers.set(host, controller);
  drawFrame(0);

  function drawFrame(index) {
    const normalized = Math.max(0, Math.min(index, points.length - 1));
    const point = points[normalized];
    focusRing.attr("cx", x(point.actual)).attr("cy", y(point.predicted));

    scatter.attr("opacity", (d) => (d.index === point.index ? 1 : 0.45));
    controller.frameIndex = normalized;
  }

  function scheduleNext() {
    if (controller.paused) {
      return;
    }
    drawFrame(controller.frameIndex + 1 >= points.length ? 0 : controller.frameIndex + 1);
    controller.timerId = setTimeout(scheduleNext, controller.intervalMs);
  }

  function play() {
    controller.paused = false;
    if (controller.timerId) {
      return;
    }
    controller.timerId = setTimeout(scheduleNext, controller.intervalMs);
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
    drawFrame(0);
    play();
  }

  function step() {
    pause();
    drawFrame(controller.frameIndex + 1 >= points.length ? 0 : controller.frameIndex + 1);
  }
}

export function playTaxiRegressionD3(host) {
  controllers.get(host)?.play();
}

export function pauseTaxiRegressionD3(host) {
  controllers.get(host)?.pause();
}

export function replayTaxiRegressionD3(host) {
  controllers.get(host)?.replay();
}

export function stepTaxiRegressionD3(host) {
  controllers.get(host)?.step();
}
