build:
	docker build -t calcevents .

sh:
	docker run -it --rm calcevents bash

clean:
	docker image rm calcevents

.PHONY: build
