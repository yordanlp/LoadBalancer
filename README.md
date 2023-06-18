# LoadBalancer

Simple Load Balancer implementation, it exposes endpoints to configure the hosts of the running instances of the application, perform health checks on the instances before redirecting the traffic to it and uses a round robing algorithm to select the next candidate instance

Endpoints to configure instances hosts:

- POST /api/Instance
{
  "host": "protocol://host:port" 
}

- DELETE: /api/Instance/{id}
- GET: /api/Instance/{id}
- GET: /api/Instance //Returns the list of instances registered

All the other routes are catched and redirected to the application running in one of the instances

### Important (HostService)

HostService is a Singleton service that has a method called UpdateHosts which is a background task that runs every minute and reads the hosts from the database and performs the health check on them and has a method to return the next host.
