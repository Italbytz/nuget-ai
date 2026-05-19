import * as d3 from "https://cdn.jsdelivr.net/npm/d3@7/+esm";

const controllers = new WeakMap();

export function renderUmbrellaWorldD3(host, model) {
  if (!host || !model || !Array.isArray(model.steps) || model.steps.length === 0) {
    return;
  }

  const previous = controllers.get(host);
  if (previous?.timerId) {
    clearTimeout(previous.timerId);
  }

  const width = Math.max(host.clientWidth || 760, 420);
  const height = Math.max(host.clientHeight || 300, 240);
  host.innerHTML = "";

  const steps = model.steps;
  const svg = d3.select(host).append("svg").attr("width", width).attr("height", height).attr("viewBox", `0 0 ${width} ${height}`);

  const margin = { top: 24, right: 18, bottom: 34, left: 42 };
  const x = d3.scaleLinear().domain([1, steps.length]).range([margin.left, width - margin.right]);
  const y = d3.scaleLinear().domain([0, 1]).range([height - margin.bottom, margin.top]);

  svg.append("g").attr("transform", `translate(0, ${height - margin.bottom})`).call(d3.axisBottom(x).ticks(steps.length).tickFormat((d) => `D${d}`));
  svg.append("g").attr("transform", `translate(${margin.left}, 0)`).call(d3.axisLeft(y).ticks(5));

  const filteredLine = d3.line().x((d) => x(d.day)).y((d) => y(d.filtered));
  const smoothedLine = d3.line().x((d) => x(d.day)).y((d) => y(d.smoothed));

  svg.append("path").datum(steps).attr("d", filteredLine).attr("fill", "none").attr("stroke", "#ea580c").attr("stroke-width", 2.2);
  svg.append("path").datum(steps).attr("d", smoothedLine).attr("fill", "none").attr("stroke", "#0f766e").attr("stroke-width", 2.2);

  const filteredMarker = svg.append("circle").attr("r", 4.4).attr("fill", "#ea580c");
  const smoothedMarker = svg.append("circle").attr("r", 4.4).attr("fill", "#0f766e");

  const obsMarkers = svg
    .append("g")
    .selectAll("text")
    .data(steps)
    .join("text")
    .attr("x", (d) => x(d.day))
    .attr("y", margin.top + 12)
    .attr("text-anchor", "middle")
    .attr("font-size", 12)
    .attr("fill", "#334155")
    .text((d) => (d.observed ? "U" : "-"));

  svg
    .append("text")
    .attr("x", margin.left)
    .attr("y", 14)
    .attr("font-size", 12)
    .attr("font-weight", 700)
    .attr("fill", "#0f172a")
    .text("Rain posterior over time");

  const controller = {
    frame: Math.max(0, Math.min((model.currentStep || 0), steps.length)),
    timerId: null,
    paused: false,
    intervalMs: 640,
    play,
    pause,
    step,
    replay
  };
  controllers.set(host, controller);

  drawFrame(controller.frame);

  function drawFrame(frame) {
    controller.frame = Math.max(0, Math.min(frame, steps.length));
    const visible = Math.max(1, controller.frame);
    const step = steps[visible - 1];

    filteredMarker.attr("cx", x(step.day)).attr("cy", y(step.filtered));
    smoothedMarker.attr("cx", x(step.day)).attr("cy", y(step.smoothed));
    obsMarkers.attr("opacity", (d) => (d.day <= controller.frame ? 1 : 0.35));
  }

  function schedule() {
    if (controller.paused) return;
    const next = controller.frame + 1 > steps.length ? 1 : controller.frame + 1;
    drawFrame(next);
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
    const next = controller.frame + 1 > steps.length ? 1 : controller.frame + 1;
    drawFrame(next);
  }

  function replay() {
    pause();
    drawFrame(1);
    play();
  }
}

export function playUmbrellaWorldD3(host) {
  controllers.get(host)?.play();
}

export function pauseUmbrellaWorldD3(host) {
  controllers.get(host)?.pause();
}

export function stepUmbrellaWorldD3(host) {
  controllers.get(host)?.step();
}

export function replayUmbrellaWorldD3(host) {
  controllers.get(host)?.replay();
}
