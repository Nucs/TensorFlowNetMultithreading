build:
	docker build -t calcevents .

sh: build
	docker run -it --rm calcevents bash

run: build
	docker run -it --rm calcevents

clean:
	docker image rm calcevents

.PHONY: build
