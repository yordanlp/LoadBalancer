# LoadBalancer

Simple Load Balancer implementation, it exposes endpoints to configure the hosts of the running instances of the application, perform health checks on the instances before redirecting the traffic to it and uses a round robing algorithm to select the next candidate instance

Endpoints to configure instances hosts:

- POST /api/Instance
{
  "host": "protocol://host:port" // http:192.168.0.1:5000
}

- DELETE: /api/Instance/{id}
- GET: api/Instance/{id}
- GET: api/Instance //Returns the list of instances registered

All the other routes are cathed and redirected to the Application running in one of the instances
