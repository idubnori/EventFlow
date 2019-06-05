# functions
Function Get-Container-Ip($containername)
{
	docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' $containername
}
# end functions

Switch-DockerLinux

# Up containers
# docker network create nat
docker-compose -f docker-compose.ci.yml pull --parallel
docker-compose -f docker-compose.ci.yml up -d

# Install curl
cinst curl -y --no-progress
sal curl (Join-Path $env:ChocolateyInstall "bin\curl.exe") -O AllScope

docker ps -a

# Set connection url to environment variable
# RabbitMQ
$rabbitmq_ip = Get-Container-Ip rabbitmq-ef
$env:RABBITMQ_URL = "amqp://guest:guest@${rabbitmq_ip}:5672"
# Elasticsearch
$elasticsearch_ip = Get-Container-Ip elasticsearch-ef
$env:ELASTICSEARCH_URL = "http://${elasticsearch_ip}:9200"
# Event Store
$eventstore_ip = Get-Container-Ip eventstore-ef
$env:EVENTSTORE_URL = "tcp://admin:changeit@${eventstore_ip}:1113"

# Helth check
# Event Store
curl --connect-timeout 60 --retry 5 -sL "http://${eventstore_ip}:2113"
# Elasticsearch
curl --connect-timeout 60 --retry 5 -sL "http://${elasticsearch_ip}:9200"
# RabbitMQ
curl --connect-timeout 60 --retry 5 -sL "http://${rabbitmq_ip}:15672"