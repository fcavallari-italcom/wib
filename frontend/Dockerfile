FROM nginx:alpine

# Install envsubst and bash
RUN apk add --no-cache bash gettext

# Select which app to serve (default: wib-wmc)
ARG APP_PATH=apps/wib-wmc

# Copy application files
COPY ${APP_PATH}/src/ /usr/share/nginx/html/

# Copy entrypoint script
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

EXPOSE 80

ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
