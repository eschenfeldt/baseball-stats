# FROM node:alpine as serve

# WORKDIR /usr/src/app
# COPY . /usr/src/app/
# RUN npm install -g @angular/cli
# RUN npm install

# CMD ["ng", "serve", "--host", "0.0.0.0"]

FROM nginx:latest as prod

EXPOSE 80
EXPOSE 443

COPY /dist/baseball-app/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf