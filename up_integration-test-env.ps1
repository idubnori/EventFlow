# functions
Function Get-Container-Ip($containername)
{
	docker inspect -f "{{ .NetworkSettings.Networks.nat.IPAddress }}" $containername
}
# end functions

# Up containers
docker-compose -f docker-compose.ci.yml up -d

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

# Install wget
cinst wget -y --no-progress
sal wget (Join-Path $env:ChocolateyInstall "bin\wget.exe") -O AllScope

# Helth check
# Event Store
wget --timeout=60 --tries=5 -O - "http://${eventstore_ip}:2113"
# Elasticsearch
wget --timeout=60 --tries=5 -O - "http://${elasticsearch_ip}:9200"
# RabbitMQ
wget --timeout=60 --tries=5 -O - "http://${rabbitmq_ip}:15672"