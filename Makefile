SHELL := /bin/bash

SOLUTION := nuget-ai.sln
CONFIGURATION ?= Debug
PACK_CONFIGURATION ?= Release
DOCFX_CONFIG := docfx/docfx.json
DEMO_PROJECT := samples/Italbytz.AI.Demos.Web/Italbytz.AI.Demos.Web.csproj
DEMO_HTTP_URL := http://localhost:5128
DEMO_PUBLISH_DIR := artifacts/demo-build
PAGES_DIR := artifacts/pages
PAGES_PORT ?= 8080
PAGES_HTTP_URL := http://localhost:$(PAGES_PORT)

.PHONY: help restore build test pack docs demo-run demo-watch demo-open demo-publish pages-prepare pages-serve pages-open feedback clean
.PHONY: pages-serve-open

help:
	@echo "Available targets:"
	@echo "  make restore        - Restore solution and tools"
	@echo "  make build          - Build the full solution"
	@echo "  make test           - Run the full test suite"
	@echo "  make pack           - Pack NuGet packages"
	@echo "  make docs           - Build the docfx site"
	@echo "  make demo-run       - Run the demo host once on $(DEMO_HTTP_URL)"
	@echo "  make demo-watch     - Run the demo host with dotnet watch on $(DEMO_HTTP_URL)"
	@echo "  make demo-open      - Open the running demo host in the browser"
	@echo "  make demo-publish   - Publish the demo host into $(DEMO_PUBLISH_DIR)"
	@echo "  make pages-prepare  - Build the combined local Pages artifact"
	@echo "  make pages-serve    - Serve the combined local Pages artifact on http://localhost:$(PAGES_PORT)"
	@echo "  make pages-open     - Open the running local Pages preview in the browser"
	@echo "  make pages-serve-open - Start a local Pages preview in the background and open it in the browser"
	@echo "  make feedback       - Fast local feedback loop: restore, build demo, docs, pages artifact"
	@echo "  make clean          - Remove build artifacts"

restore:
	dotnet tool restore
	dotnet restore $(SOLUTION)

build: restore
	dotnet build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

test: restore
	dotnet test $(SOLUTION) --configuration $(CONFIGURATION) --no-restore --verbosity minimal

pack: restore
	dotnet pack $(SOLUTION) --configuration $(PACK_CONFIGURATION) --no-restore --verbosity minimal --output ./artifacts/packages

docs:
	dotnet tool restore
	dotnet tool run docfx $(DOCFX_CONFIG)

demo-run:
	dotnet run --project $(DEMO_PROJECT) --launch-profile http

demo-watch:
	dotnet watch --project $(DEMO_PROJECT) run --launch-profile http

demo-open:
	open $(DEMO_HTTP_URL)

demo-publish:
	dotnet publish $(DEMO_PROJECT) --configuration $(PACK_CONFIGURATION) --output ./$(DEMO_PUBLISH_DIR)

pages-prepare: docs demo-publish
	rm -rf ./$(PAGES_DIR)
	mkdir -p ./$(PAGES_DIR)/demos
	perl -0pi -e 's#<base href="/" />#<base href="/nuget-ai/demos/" />#' ./$(DEMO_PUBLISH_DIR)/wwwroot/index.html
	cp ./$(DEMO_PUBLISH_DIR)/wwwroot/index.html ./$(DEMO_PUBLISH_DIR)/wwwroot/404.html
	cp -R ./docfx/_site/. ./$(PAGES_DIR)
	cp -R ./$(DEMO_PUBLISH_DIR)/wwwroot/. ./$(PAGES_DIR)/demos
	touch ./$(PAGES_DIR)/.nojekyll
	@echo "Pages artifact ready in ./$(PAGES_DIR)"

pages-serve: pages-prepare
	cd ./$(PAGES_DIR) && python3 -m http.server $(PAGES_PORT)

pages-open:
	open $(PAGES_HTTP_URL)

pages-serve-open: pages-prepare
	cd ./$(PAGES_DIR) && nohup python3 -m http.server $(PAGES_PORT) >/tmp/nuget-ai-pages-serve.log 2>&1 &
	@sleep 1
	open $(PAGES_HTTP_URL)
	@echo "Pages preview started in the background on $(PAGES_HTTP_URL)"
	@echo "Log: /tmp/nuget-ai-pages-serve.log"

feedback: restore demo-publish docs pages-prepare
	@echo "Demo host: $(DEMO_HTTP_URL) via 'make demo-watch' and 'make demo-open'"
	@echo "Pages preview: $(PAGES_HTTP_URL) via 'make pages-serve', 'make pages-open', or 'make pages-serve-open'"

clean:
	rm -rf ./artifacts/demo-build ./artifacts/pages
	dotnet clean $(SOLUTION)