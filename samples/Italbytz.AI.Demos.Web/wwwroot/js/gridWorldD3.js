import * as d3 from "https://cdn.jsdelivr.net/npm/d3@7/+esm";

const controllers = new WeakMap();

function actionArrow(action) {
  switch (action) {
    case "Up":
      return "U";
    case "Down":
      return "D";
    case "Left":
      return "L";
    case "Right":
      return "R";
    default:
      return "";
  }
}

export function renderGridWorldD3(host, model) {
  if (!host || !model || !Array.isArray(model.states)) {
    return;
  }

  const width = Math.max(host.clientWidth || 760, 420);
  const height = Math.max(host.clientHeight || 300, 250);
  host.innerHTML = "";
  const previous = controllers.get(host);
  if (previous?.timerId) {
    clearTimeout(previous.timerId);
  }

  const svg = d3.select(host).append("svg").attr("width", width).attr("height", height).attr("viewBox", `0 0 ${width} ${height}`);

  const heatWidth = Math.round(width * 0.62);
  const margin = { top: 16, right: 18, bottom: 20, left: 16 };
  const cols = 4;
  const rows = 3;
  const cellW = (heatWidth - margin.left - margin.right) / cols;
  const cellH = (height - margin.top - margin.bottom) / rows;

  const shownUtilities = model.states.filter((s) => s.utility !== null).map((s) => s.utility);
  const minU = shownUtilities.length ? Math.min(...shownUtilities) : -1;
  const maxU = shownUtilities.length ? Math.max(...shownUtilities) : 1;
  const color = d3.scaleLinear().domain([minU, 0, maxU]).range(["#e76f51", "#f8fafc", "#2a9d8f"]);

  const grid = svg.append("g");

  model.states.forEach((state) => {
    const x = margin.left + (state.col - 1) * cellW;
    const y = margin.top + (state.row - 1) * cellH;

    grid
      .append("rect")
      .attr("x", x)
      .attr("y", y)
      .attr("width", cellW - 4)
      .attr("height", cellH - 4)
      .attr("rx", 10)
      .attr("fill", state.isWall ? "#cbd5e1" : state.isVisible ? color(state.utility ?? 0) : "#e2e8f0")
      .attr("stroke", state.isCurrent ? "#0f172a" : "#475569")
      .attr("stroke-width", state.isCurrent ? 2.5 : 1);

    if (state.isWall) {
      grid
        .append("text")
        .attr("x", x + (cellW - 4) / 2)
        .attr("y", y + cellH / 2)
        .attr("text-anchor", "middle")
        .attr("dominant-baseline", "middle")
        .attr("fill", "#334155")
        .attr("font-size", 12)
        .attr("font-weight", 600)
        .text("Wall");
      return;
    }

    grid
      .append("text")
      .attr("x", x + 10)
      .attr("y", y + 16)
      .attr("fill", "#1e293b")
      .attr("font-size", 11)
      .text(state.state);

    if (state.isVisible) {
      grid
        .append("text")
        .attr("x", x + 10)
        .attr("y", y + cellH / 2 + 4)
        .attr("fill", "#0f172a")
        .attr("font-size", 13)
        .attr("font-weight", 700)
        .text((state.utility ?? 0).toFixed(3));

      grid
        .append("text")
        .attr("x", x + cellW - 18)
        .attr("y", y + cellH - 12)
        .attr("text-anchor", "middle")
        .attr("fill", "#0f172a")
        .attr("font-size", 16)
        .text(actionArrow(state.action));
    }
  });

  const chartX = heatWidth + 14;
  const chartWidth = width - chartX - 18;
  const chartTop = 30;
  const chartBottom = height - 28;
  const chartHeight = chartBottom - chartTop;

  svg
    .append("text")
    .attr("x", chartX)
    .attr("y", 18)
    .attr("fill", "#0f172a")
    .attr("font-size", 12)
    .attr("font-weight", 700)
    .text("Running utility profile");

  const profile = Array.isArray(model.runningProfile) ? model.runningProfile : [];
  let markerCircle = null;
  if (profile.length > 1) {
    const xScale = d3.scaleLinear().domain([1, profile.length]).range([chartX, chartX + chartWidth]);
    const yExtent = d3.extent(profile, (d) => d.meanUtility);
    const yPad = ((yExtent[1] ?? 0) - (yExtent[0] ?? 0)) * 0.15 || 0.1;
    const yScale = d3.scaleLinear().domain([(yExtent[0] ?? 0) - yPad, (yExtent[1] ?? 0) + yPad]).range([chartBottom, chartTop]);

    const line = d3
      .line()
      .x((d) => xScale(d.index))
      .y((d) => yScale(d.meanUtility));

    svg
      .append("path")
      .datum(profile)
      .attr("d", line)
      .attr("fill", "none")
      .attr("stroke", "#1d4ed8")
      .attr("stroke-width", 2.2);

    markerCircle = svg.append("circle").attr("r", 4.2).attr("fill", "#1d4ed8");

    const visibleStep = Math.max(0, Math.min(model.currentStep, profile.length));
    if (visibleStep > 0) {
      const marker = profile[visibleStep - 1];
      markerCircle.attr("cx", xScale(marker.index)).attr("cy", yScale(marker.meanUtility));
    } else {
      markerCircle.attr("cx", xScale(profile[0].index)).attr("cy", yScale(profile[0].meanUtility));
    }

    svg
      .append("line")
      .attr("x1", chartX)
      .attr("x2", chartX + chartWidth)
      .attr("y1", chartBottom)
      .attr("y2", chartBottom)
      .attr("stroke", "#64748b");

    const controller = {
      frameIndex: Math.max(0, Math.min(visibleStep > 0 ? visibleStep - 1 : 0, profile.length - 1)),
      timerId: null,
      paused: false,
      intervalMs: 560,
      play,
      pause,
      replay,
      step
    };
    controllers.set(host, controller);

    function drawFrame(index) {
      const normalized = Math.max(0, Math.min(index, profile.length - 1));
      const frame = profile[normalized];
      markerCircle.attr("cx", xScale(frame.index)).attr("cy", yScale(frame.meanUtility));
      controller.frameIndex = normalized;
    }

    function scheduleNext() {
      if (controller.paused) {
        return;
      }
      drawFrame(controller.frameIndex + 1 >= profile.length ? 0 : controller.frameIndex + 1);
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
      drawFrame(controller.frameIndex + 1 >= profile.length ? 0 : controller.frameIndex + 1);
    }
  }
}

export function playGridWorldD3(host) {
  controllers.get(host)?.play();
}

export function pauseGridWorldD3(host) {
  controllers.get(host)?.pause();
}

export function replayGridWorldD3(host) {
  controllers.get(host)?.replay();
}

export function stepGridWorldD3(host) {
  controllers.get(host)?.step();
}
