# functions
Function Get-Container-Ip($containername)
{
	docker inspect -f "{{ .NetworkSettings.Networks.nat.IPAddress }}" $containername
}
# end functions

# Up containers
docker-compose -f docker-compose.ci.yml pull --parallel 2>&1 | %{ "$_" }
docker-compose -f docker-compose.ci.yml up -d 2>&1 | %{ "$_" }

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

# Check ready
# Event Store
curl "http://${eventstore_ip}:2113" -sSL --connect-timeout 90
# Elasticsearch
curl "http://${elasticsearch_ip}:9200" -sSL --connect-timeout 90
# RabbitMQ
curl "http://${rabbitmq_ip}:15672" -sSL --connect-timeout 90