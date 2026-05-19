import * as d3 from "https://cdn.jsdelivr.net/npm/d3@7/+esm";

const controllers = new WeakMap();

export function renderBlockworldPlanningD3(host, model) {
  if (!host || !model || !Array.isArray(model.steps) || model.steps.length === 0) return;

  const previous = controllers.get(host);
  if (previous?.timerId) clearTimeout(previous.timerId);

  const steps = model.steps;
  const width = Math.max(host.clientWidth || 760, 420);
  const height = Math.max(host.clientHeight || 290, 230);
  host.innerHTML = "";

  const svg = d3.select(host).append("svg").attr("width", width).attr("height", height).attr("viewBox", `0 0 ${width} ${height}`);
  const margin = { top: 24, right: 18, bottom: 32, left: 42 };

  const x = d3.scaleLinear().domain([1, steps.length]).range([margin.left, width - margin.right]);
  const y = d3.scaleLinear().domain([0, d3.max(steps, (d) => d.f) || 1]).nice().range([height - margin.bottom, margin.top]);

  svg.append("g").attr("transform", `translate(0, ${height - margin.bottom})`).call(d3.axisBottom(x).ticks(Math.min(10, steps.length)));
  svg.append("g").attr("transform", `translate(${margin.left}, 0)`).call(d3.axisLeft(y).ticks(6));

  svg
    .append("path")
    .datum(steps)
    .attr("d", d3.line().x((d) => x(d.number)).y((d) => y(d.f)))
    .attr("fill", "none")
    .attr("stroke", "#7c3aed")
    .attr("stroke-width", 2.3);

  const marker = svg.append("circle").attr("r", 5).attr("fill", "#0f172a");
  const label = svg.append("text").attr("x", margin.left).attr("y", 14).attr("font-size", 12).attr("font-weight", 700).attr("fill", "#0f172a");

  const controller = { frame: Math.max(1, Math.min(model.currentStep || 0, steps.length)), timerId: null, paused: false, intervalMs: 650, play, pause, step, replay };
  controllers.set(host, controller);
  drawFrame(controller.frame || 1);

  function drawFrame(frame) {
    const clamped = Math.max(1, Math.min(frame, steps.length));
    controller.frame = clamped;
    const item = steps[clamped - 1];
    marker.attr("cx", x(item.number)).attr("cy", y(item.f));
    label.text(item.isGoal ? `Goal reached at step ${item.number}` : `g=${item.g}, h=${item.h}, f=${item.f}, succ=${item.successors}`);
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

export function playBlockworldPlanningD3(host) { controllers.get(host)?.play(); }
export function pauseBlockworldPlanningD3(host) { controllers.get(host)?.pause(); }
export function stepBlockworldPlanningD3(host) { controllers.get(host)?.step(); }
export function replayBlockworldPlanningD3(host) { controllers.get(host)?.replay(); }
