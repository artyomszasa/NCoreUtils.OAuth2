
NAME=ncoreutils-oauth2
VERSION?=2.0.0.1-local
PREFIX?=eu.gcr.io/hosting-666

build-image:
	docker build -t $(PREFIX)/$(NAME):$(VERSION) .

push-image: build-image
	docker push $(PREFIX)/$(NAME):$(VERSION)
