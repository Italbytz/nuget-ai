import * as d3 from "https://cdn.jsdelivr.net/npm/d3@7/+esm";

const controllers = new WeakMap();

export function renderCykParserD3(host, model) {
  if (!host || !model || !Array.isArray(model.cells)) {
    return;
  }

  const previous = controllers.get(host);
  if (previous?.timerId) {
    clearTimeout(previous.timerId);
  }

  const width = Math.max(host.clientWidth || 760, 420);
  const height = Math.max(host.clientHeight || 300, 240);
  host.innerHTML = "";

  const words = Array.isArray(model.words) ? model.words : [];
  const n = Math.max(1, words.length);
  const cells = model.cells;

  const svg = d3.select(host).append("svg").attr("width", width).attr("height", height).attr("viewBox", `0 0 ${width} ${height}`);
  const margin = { top: 20, right: 16, bottom: 28, left: 16 };
  const cellW = (width - margin.left - margin.right) / n;
  const cellH = (height - margin.top - margin.bottom) / n;

  const maxCount = d3.max(cells, (c) => c.count) || 1;
  const color = d3.scaleLinear().domain([0, maxCount]).range(["#eef2f7", "#1d4ed8"]);

  const cellByNumber = new Map(cells.map((c) => [c.number, c]));

  const all = svg
    .append("g")
    .selectAll("g")
    .data(cells)
    .join("g")
    .attr("transform", (d) => {
      const x = margin.left + d.start * cellW;
      const y = margin.top + (n - d.span) * cellH;
      return `translate(${x},${y})`;
    });

  all
    .append("rect")
    .attr("width", cellW - 4)
    .attr("height", cellH - 4)
    .attr("rx", 8)
    .attr("fill", (d) => color(d.count))
    .attr("stroke", "#475569")
    .attr("stroke-width", 1)
    .attr("opacity", 0.28);

  all
    .append("text")
    .attr("x", 8)
    .attr("y", 14)
    .attr("fill", "#0f172a")
    .attr("font-size", 10)
    .text((d) => d.label);

  all
    .append("text")
    .attr("x", 8)
    .attr("y", 30)
    .attr("fill", "#0f172a")
    .attr("font-size", 11)
    .attr("font-weight", 700)
    .text((d) => `NT: ${d.count}`);

  const focus = svg
    .append("rect")
    .attr("rx", 8)
    .attr("fill", "none")
    .attr("stroke", "#0f172a")
    .attr("stroke-width", 2)
    .attr("stroke-dasharray", "4,3");

  const maxStep = cells.length;
  const controller = {
    frame: Math.max(0, Math.min((model.currentStep || 0), maxStep)),
    timerId: null,
    paused: false,
    intervalMs: 620,
    play,
    pause,
    step,
    replay
  };
  controllers.set(host, controller);

  drawFrame(controller.frame);

  function drawFrame(frame) {
    controller.frame = Math.max(0, Math.min(frame, maxStep));

    all.selectAll("rect").attr("opacity", (d) => (d.number <= controller.frame ? 0.95 : 0.18));

    if (controller.frame <= 0) {
      focus.attr("width", 0).attr("height", 0);
      return;
    }

    const cell = cellByNumber.get(controller.frame);
    if (!cell) {
      return;
    }

    const x = margin.left + cell.start * cellW;
    const y = margin.top + (n - cell.span) * cellH;
    focus.attr("x", x).attr("y", y).attr("width", cellW - 4).attr("height", cellH - 4);
  }

  function schedule() {
    if (controller.paused) {
      return;
    }

    const next = controller.frame + 1 > maxStep ? 0 : controller.frame + 1;
    drawFrame(next);
    controller.timerId = setTimeout(schedule, controller.intervalMs);
  }

  function play() {
    controller.paused = false;
    if (controller.timerId) {
      return;
    }
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
    drawFrame(controller.frame + 1 > maxStep ? 0 : controller.frame + 1);
  }

  function replay() {
    pause();
    drawFrame(0);
    play();
  }
}

export function playCykParserD3(host) {
  controllers.get(host)?.play();
}

export function pauseCykParserD3(host) {
  controllers.get(host)?.pause();
}

export function stepCykParserD3(host) {
  controllers.get(host)?.step();
}

export function replayCykParserD3(host) {
  controllers.get(host)?.replay();
}
