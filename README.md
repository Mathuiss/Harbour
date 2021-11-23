# Harbour
Simple deployment platform for dockerized applications

Harbour manages your dockerized application. Harbour's only dependency is docker. To install docker visit [Docker install page](https://docs.docker.com/engine/install/).

### Usage

Harbour works on the idea that you can apply a state to your machine. Harbour will then attempt to apply the docker configuration to your docker environment. This means that harbour can run and remove containers, serve HTTP endpoints tied to containers, add new services and removing services.

```bash
# Example of applying default state
sudo harbour apply

# Example of applying a new state
sudo harbour apply state.json

# Example of adding a new service
sudo harbour add service.json

# Example of removing a service
sudo harbour remove service-name

# Example of enabling HTTP endpoints on port 80 with current state
sudo harbour serve

# Example of enabling HTTP endpoints on port 80 with new state
sudo harbour serve state.json

# Example of stopping HTTP server
sudo harbour serve stop

# Run server in the background
sudo harbour serve -d
sudo harbour serve --detached

# Get help
harbour -h
harbour --help
```

### Behaviour

Harbour updates the current-state.json each time changes are made. If a service is added or removed, these changes are visible in the ```current-state.json```. This means that users can edit the current-state.json file if small changes need to be made. Be careful, if harbour is unable to read it's ```current-state.json```, it will be unable to run. This is why you can supply a new ```state.json``` to test the configuration file, before any changes are made.

### Built-in proxy server
Harbour conains a built-in proxy server. It tries to bind to port 80 and can be activated by running ```sudo harbour serve```. The server proxies all incoming HTTP traffic to the corresponding containers as specified in the ```current-state.json```. Harbour user the Kestrel web server and an asynchronous HTTP client to proxy incoming trafic to the containers. The server can also be started in the background with ```sudo harbour serve -d```. This will spawn a new process and run the server until ```sudo harbour serve stop``` is called.