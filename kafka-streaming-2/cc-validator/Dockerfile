FROM node:12.12-alpine
WORKDIR /app
COPY package.json ./
RUN npm install
COPY . .
CMD npm start