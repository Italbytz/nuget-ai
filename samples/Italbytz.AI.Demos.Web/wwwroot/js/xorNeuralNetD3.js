import * as d3 from "https://cdn.jsdelivr.net/npm/d3@7/+esm";

const controllers = new WeakMap();

export function renderXorNeuralNetD3(host, model) {
  if (!host || !model || !Array.isArray(model.loss) || !Array.isArray(model.points)) {
    return;
  }

  const previous = controllers.get(host);
  if (previous?.timerId) {
    clearTimeout(previous.timerId);
  }

  const width = Math.max(host.clientWidth || 760, 420);
  const height = Math.max(host.clientHeight || 330, 260);
  host.innerHTML = "";

  const svg = d3.select(host).append("svg").attr("width", width).attr("height", height).attr("viewBox", `0 0 ${width} ${height}`);

  const leftWidth = Math.round(width * 0.62);
  const margin = { top: 28, right: 20, bottom: 36, left: 48 };
  const chartW = leftWidth - margin.left - margin.right;
  const chartH = height - margin.top - margin.bottom;

  const loss = model.loss;
  const points = model.points;

  const x = d3.scaleLinear().domain(d3.extent(loss, (d) => d.epoch)).range([margin.left, margin.left + chartW]);
  const y = d3.scaleLinear().domain([0, d3.max(loss, (d) => d.mse) || 1]).nice().range([height - margin.bottom, margin.top]);

  svg.append("g").attr("transform", `translate(0, ${height - margin.bottom})`).call(d3.axisBottom(x).ticks(6));
  svg.append("g").attr("transform", `translate(${margin.left}, 0)`).call(d3.axisLeft(y).ticks(6));

  svg
    .append("path")
    .datum(loss)
    .attr("d", d3.line().x((d) => x(d.epoch)).y((d) => y(d.mse)))
    .attr("fill", "none")
    .attr("stroke", "#1d4ed8")
    .attr("stroke-width", 2.2);

  const lossMarker = svg.append("circle").attr("r", 4.5).attr("fill", "#1d4ed8");

  svg
    .append("text")
    .attr("x", margin.left)
    .attr("y", 16)
    .attr("font-size", 12)
    .attr("font-weight", 700)
    .attr("fill", "#0f172a")
    .text("Loss curve (MSE)");

  const paneX = leftWidth + 18;
  const paneW = width - paneX - 18;
  const paneY = margin.top;
  const paneH = chartH;

  svg
    .append("rect")
    .attr("x", paneX)
    .attr("y", paneY)
    .attr("width", paneW)
    .attr("height", paneH)
    .attr("rx", 8)
    .attr("fill", "#f8fafc")
    .attr("stroke", "#94a3b8");

  svg
    .append("text")
    .attr("x", paneX + 8)
    .attr("y", 16)
    .attr("font-size", 12)
    .attr("font-weight", 700)
    .attr("fill", "#0f172a")
    .text("XOR outputs");

  const px = d3.scaleLinear().domain([0, 1]).range([paneX + 40, paneX + paneW - 20]);
  const py = d3.scaleLinear().domain([0, 1]).range([paneY + paneH - 28, paneY + 20]);

  const dots = svg
    .append("g")
    .selectAll("circle")
    .data(points)
    .join("circle")
    .attr("cx", (d) => px(d.x1))
    .attr("cy", (d) => py(d.x2))
    .attr("r", 11)
    .attr("fill", (d) => (d.predicted === d.expected ? "#22c55e" : "#ef4444"))
    .attr("stroke", "#0f172a")
    .attr("stroke-width", 1.2);

  const labels = svg
    .append("g")
    .selectAll("text")
    .data(points)
    .join("text")
    .attr("x", (d) => px(d.x1))
    .attr("y", (d) => py(d.x2) + 4)
    .attr("text-anchor", "middle")
    .attr("font-size", 10)
    .attr("fill", "#0f172a")
    .text((d) => d.raw.toFixed(2));

  const focusRing = svg.append("circle").attr("r", 16).attr("fill", "none").attr("stroke", "#0f172a").attr("stroke-dasharray", "4,3");

  const frameCount = Math.max(loss.length, points.length);
  const controller = {
    frame: 0,
    timerId: null,
    paused: false,
    intervalMs: 620,
    play,
    pause,
    step,
    replay
  };
  controllers.set(host, controller);
  drawFrame(0);

  function drawFrame(frame) {
    controller.frame = ((frame % frameCount) + frameCount) % frameCount;

    const lossItem = loss[Math.min(controller.frame, loss.length - 1)];
    if (lossItem) {
      lossMarker.attr("cx", x(lossItem.epoch)).attr("cy", y(lossItem.mse));
    }

    const point = points[controller.frame % points.length];
    focusRing.attr("cx", px(point.x1)).attr("cy", py(point.x2));
    dots.attr("opacity", (d) => (d.index === point.index ? 1 : 0.42));
    labels.attr("opacity", (d) => (d.index === point.index ? 1 : 0.55));
  }

  function schedule() {
    if (controller.paused) return;
    drawFrame(controller.frame + 1);
    controller.timerId = setTimeout(schedule, controller.intervalMs);
  }

  function play() {
    controller.paused = false;
    if (controller.timerId) return;
    controller.timerId = setTimeout(schedule, controller.intervalMs);
  }

  function pause() {
    controller.paused = true;
    if (controller.timerId) {
      clearTimeout(controller.timerId);
      controller.timerId = null;
    }
  }

  function step() {
    pause();
    drawFrame(controller.frame + 1);
  }

  function replay() {
    pause();
    drawFrame(0);
    play();
  }
}

export function playXorNeuralNetD3(host) {
  controllers.get(host)?.play();
}

export function pauseXorNeuralNetD3(host) {
  controllers.get(host)?.pause();
}

export function stepXorNeuralNetD3(host) {
  controllers.get(host)?.step();
}

export function replayXorNeuralNetD3(host) {
  controllers.get(host)?.replay();
}
