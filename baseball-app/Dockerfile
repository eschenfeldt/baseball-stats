FROM node:alpine AS build

WORKDIR /usr/src/app
COPY package.json package-lock.json ./
# install node packages before copying files to avoid copying node_modules
RUN npm install 
COPY . .
RUN npm run build

FROM nginx:latest AS prod

EXPOSE 80
EXPOSE 443

COPY --from=build /usr/src/app/dist/baseball-app/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf