import * as d3 from "https://cdn.jsdelivr.net/npm/d3@7/+esm";

const controllers = new WeakMap();

export function renderNQueensSearchD3(host, model) {
  if (!host || !model || !Array.isArray(model.steps) || model.steps.length === 0) return;

  const previous = controllers.get(host);
  if (previous?.timerId) clearTimeout(previous.timerId);

  const steps = model.steps;
  const width = Math.max(host.clientWidth || 760, 420);
  const height = Math.max(host.clientHeight || 280, 220);
  host.innerHTML = "";

  const svg = d3.select(host).append("svg").attr("width", width).attr("height", height).attr("viewBox", `0 0 ${width} ${height}`);
  const margin = { top: 24, right: 16, bottom: 30, left: 44 };

  const x = d3.scaleLinear().domain([1, steps.length]).range([margin.left, width - margin.right]);
  const y = d3.scaleLinear().domain([0, d3.max(steps, (d) => d.conflicts) || 1]).nice().range([height - margin.bottom, margin.top]);

  svg.append("g").attr("transform", `translate(0, ${height - margin.bottom})`).call(d3.axisBottom(x).ticks(Math.min(10, steps.length)));
  svg.append("g").attr("transform", `translate(${margin.left}, 0)`).call(d3.axisLeft(y).ticks(6));

  svg
    .append("path")
    .datum(steps)
    .attr("d", d3.line().x((d) => x(d.number)).y((d) => y(d.conflicts)))
    .attr("fill", "none")
    .attr("stroke", "#2563eb")
    .attr("stroke-width", 2.4);

  const marker = svg.append("circle").attr("r", 5).attr("fill", "#0f172a");

  const stateLabel = svg.append("text").attr("x", margin.left).attr("y", 14).attr("fill", "#0f172a").attr("font-size", 12).attr("font-weight", 700);

  const controller = { frame: Math.max(1, Math.min(model.currentStep || 0, steps.length)), timerId: null, paused: false, intervalMs: 620, play, pause, step, replay };
  controllers.set(host, controller);
  drawFrame(controller.frame || 1);

  function drawFrame(frame) {
    const clamped = Math.max(1, Math.min(frame, steps.length));
    controller.frame = clamped;
    const item = steps[clamped - 1];
    marker.attr("cx", x(item.number)).attr("cy", y(item.conflicts));
    stateLabel.text(item.solved ? "Solved" : item.stopped ? "Stopped" : `Conflicts: ${item.conflicts}`);
  }

  function schedule() {
    if (controller.paused) return;
    drawFrame(controller.frame + 1 > steps.length ? 1 : controller.frame + 1);
    controller.timerId = setTimeout(schedule, controller.intervalMs);
  }

  function play() { controller.paused = false; if (!controller.timerId) controller.timerId = setTimeout(schedule, controller.intervalMs); }
  function pause() { controller.paused = true; if (controller.timerId) { clearTimeout(controller.timerId); controller.timerId = null; } }
  function step() { pause(); drawFrame(controller.frame + 1 > steps.length ? 1 : controller.frame + 1); }
  function replay() { pause(); drawFrame(1); play(); }
}

export function playNQueensSearchD3(host) { controllers.get(host)?.play(); }
export function pauseNQueensSearchD3(host) { controllers.get(host)?.pause(); }
export function stepNQueensSearchD3(host) { controllers.get(host)?.step(); }
export function replayNQueensSearchD3(host) { controllers.get(host)?.replay(); }
