
NAME=ncoreutils-oauth2
VERSION?=1.2.1-alpha2
PREFIX?=gcr.io/hosting-666

build-image:
	docker build -t $(PREFIX)/$(NAME):$(VERSION) .

push-image: build-image
	docker push $(PREFIX)/$(NAME):$(VERSION)
