## Requires Docker 17.0.5+
## Adapted from https://github.com/spara/java-servlet-example

# build servlet and create war file
FROM maven:latest AS warfile
WORKDIR /usr/src/saasi
COPY pom.xml .
RUN mvn -B -f pom.xml -s /usr/share/maven/ref/settings-docker.xml dependency:resolve
COPY . .
RUN mvn -B -s /usr/share/maven/ref/settings-docker.xml package


FROM tomcat:9.0-jre8-alpine
# ADD tomcat/catalina.sh $CATALINA_HOME/bin/
WORKDIR /usr/local/tomcat/bin
COPY run.sh run.sh
RUN chmod +x run.sh
#Copy war file
WORKDIR /usr/local/tomcat/webapps
RUN rm -rf *
COPY  --from=warfile /usr/src/saasi/target/java-servlet-example-0.0.1-SNAPSHOT.war ROOT.war
# Expose ports
ENV JPDA_ADDRESS="8000"
ENV JPDA_TRANSPORT="dt_socket"
EXPOSE 8080
WORKDIR /usr/local/tomcat/bin
CMD ["run.sh"]