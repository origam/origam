group "default" {
  targets = ["server", "e2e"]
}

target "server" {
  context = "./docker"
  dockerfile = "DockerfileServer.linux"
  cache-from = ["type=gha,scope=origam-server"]
  cache-to = ["type=gha,scope=origam-server,mode=max"]
}

target "runtime" {
  inherits = ["server"]
  target = "base"
  cache-to = []
}

target "e2e" {
  context = "./test"
  dockerfile = "Dockerfile.tests"
  contexts = {
    origam-server = "target:server"
    origam-runtime = "target:runtime"
  }
  args = {
    SERVER_IMAGE = "origam-server"
    SERVER_RUNTIME_IMAGE = "origam-runtime"
  }
  tags = ["origam-e2e:latest"]
  cache-from = ["type=gha,scope=origam-e2e"]
  cache-to = ["type=gha,scope=origam-e2e,mode=max"]
}
