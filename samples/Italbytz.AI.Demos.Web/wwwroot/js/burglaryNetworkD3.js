import * as d3 from "https://cdn.jsdelivr.net/npm/d3@7/+esm";

const controllers = new WeakMap();

const links = [
  { source: "Burglary", target: "Alarm" },
  { source: "Earthquake", target: "Alarm" },
  { source: "Alarm", target: "JohnCalls" },
  { source: "Alarm", target: "MaryCalls" }
];

function stateColor(value) {
  if (value === "true") return "#86efac";
  if (value === "false") return "#fecaca";
  if (value === "query") return "#bfdbfe";
  return "#e2e8f0";
}

export function renderBurglaryD3(host, model) {
  if (!host || !model) {
    return;
  }

  const width = Math.max(host.clientWidth || 760, 420);
  const height = Math.max(host.clientHeight || 320, 250);
  host.innerHTML = "";
  const previous = controllers.get(host);
  if (previous?.timerId) {
    clearTimeout(previous.timerId);
  }

  const svg = d3.select(host).append("svg").attr("width", width).attr("height", height).attr("viewBox", `0 0 ${width} ${height}`);

  const evidenceMap = new Map((model.evidence || []).map((entry) => [entry.key, entry.value]));
  const nodes = ["Burglary", "Earthquake", "Alarm", "JohnCalls", "MaryCalls"].map((name) => ({ name, value: evidenceMap.get(name) ?? "unknown" }));

  const leftWidth = Math.round(width * 0.56);
  const positions = {
    Burglary: [70, 48],
    Earthquake: [leftWidth - 70, 48],
    Alarm: [leftWidth / 2, 130],
    JohnCalls: [90, 220],
    MaryCalls: [leftWidth - 90, 220]
  };

  const graph = svg.append("g");

  links.forEach((link) => {
    const [x1, y1] = positions[link.source];
    const [x2, y2] = positions[link.target];
    graph
      .append("line")
      .attr("x1", x1)
      .attr("y1", y1)
      .attr("x2", x2)
      .attr("y2", y2)
      .attr("stroke", "#64748b")
      .attr("stroke-width", 2.2)
      .attr("marker-end", "url(#arrow)");
  });

  svg
    .append("defs")
    .append("marker")
    .attr("id", "arrow")
    .attr("viewBox", "0 -5 10 10")
    .attr("refX", 8)
    .attr("refY", 0)
    .attr("markerWidth", 6)
    .attr("markerHeight", 6)
    .attr("orient", "auto")
    .append("path")
    .attr("d", "M0,-5L10,0L0,5")
    .attr("fill", "#64748b");

  nodes.forEach((node) => {
    const [x, y] = positions[node.name];
    graph
      .append("rect")
      .attr("x", x - 54)
      .attr("y", y - 24)
      .attr("width", 108)
      .attr("height", 48)
      .attr("rx", 10)
      .attr("fill", stateColor(node.value))
      .attr("stroke", "#334155");

    graph
      .append("text")
      .attr("x", x)
      .attr("y", y - 4)
      .attr("text-anchor", "middle")
      .attr("fill", "#0f172a")
      .attr("font-size", 12)
      .attr("font-weight", 700)
      .text(node.name);

    graph
      .append("text")
      .attr("x", x)
      .attr("y", y + 12)
      .attr("text-anchor", "middle")
      .attr("fill", "#334155")
      .attr("font-size", 11)
      .text(node.value);
  });

  const methods = Array.isArray(model.methods) ? model.methods : [];
  const barX = leftWidth + 18;
  const barWidth = width - barX - 18;

  svg
    .append("text")
    .attr("x", barX)
    .attr("y", 20)
    .attr("fill", "#0f172a")
    .attr("font-size", 12)
    .attr("font-weight", 700)
    .text("P(Burglary=true) by method");

  const y = d3
    .scaleBand()
    .domain(methods.map((m) => m.name))
    .range([42, height - 56])
    .padding(0.2);

  const x = d3.scaleLinear().domain([0, 1]).range([barX, barX + barWidth]);

  svg
    .append("line")
    .attr("x1", barX)
    .attr("x2", barX + barWidth)
    .attr("y1", height - 42)
    .attr("y2", height - 42)
    .attr("stroke", "#64748b");

  methods.forEach((method) => {
    const yy = y(method.name) ?? 0;
    const w = x(method.pTrue) - barX;
    svg
      .append("rect")
      .attr("x", barX)
      .attr("y", yy)
      .attr("width", Math.max(0, w))
      .attr("height", y.bandwidth())
      .attr("rx", 6)
      .attr("fill", method.approximate ? "#f59e0b" : "#3b82f6");

    svg
      .append("text")
      .attr("x", barX + 4)
      .attr("y", yy + y.bandwidth() / 2 + 4)
      .attr("fill", "#0f172a")
      .attr("font-size", 11)
          .text(`delta ${method.delta.toFixed(3)}`);

    if (method.approximate) {

  const focusBox = svg
    .append("rect")
    .attr("x", barX - 2)
    .attr("y", 0)
    .attr("width", barWidth + 4)
    .attr("height", 0)
    .attr("rx", 6)
    .attr("fill", "none")
    .attr("stroke", "#0f172a")
    .attr("stroke-width", 1.3)
    .attr("stroke-dasharray", "4,3")
    .style("opacity", methods.length > 0 ? 1 : 0);

  if (methods.length === 0) {
    return;
  }

  const controller = {
    frameIndex: 0,
    timerId: null,
    paused: false,
    intervalMs: 650,
    play,
    pause,
    replay,
    step
  };
  controllers.set(host, controller);
  drawFrame(0);

  function drawFrame(index) {
    const normalized = Math.max(0, Math.min(index, methods.length - 1));
    const method = methods[normalized];
    const yy = y(method.name) ?? 0;
    focusBox.attr("y", yy - 2).attr("height", y.bandwidth() + 4);
    controller.frameIndex = normalized;
  }

  function scheduleNext() {
    if (controller.paused) {
      return;
    }
    drawFrame(controller.frameIndex + 1 >= methods.length ? 0 : controller.frameIndex + 1);
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
    drawFrame(controller.frameIndex + 1 >= methods.length ? 0 : controller.frameIndex + 1);
  }
      svg

export function playBurglaryD3(host) {
  controllers.get(host)?.play();
}

export function pauseBurglaryD3(host) {
  controllers.get(host)?.pause();
}

export function replayBurglaryD3(host) {
  controllers.get(host)?.replay();
}

export function stepBurglaryD3(host) {
  controllers.get(host)?.step();
}
        .append("text")
        .attr("x", x(Math.min(1, method.pTrue + 0.06)))
        .attr("y", yy + y.bandwidth() / 2 + 4)
        .attr("fill", "#7c2d12")
        .attr("font-size", 10)
        .text(` ${method.delta.toFixed(3)}`);
    }
  });
}
