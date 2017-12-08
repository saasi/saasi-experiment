## SmartVM experiments

## Folder structure
- `controlvm` Code to run on the **master** node
  - `globalMonitor`
  - `tools`
- `workvm` Code to run on the **slave** nodes
  - `Application1`
  - `Application2`
  - `Application3`
    - `microservices` The actual load, like API microservices and Business service
    - `controllers` The controlling/supporting services such as Monitor and DM
    - `script_agent` The agent for running bash script on the host (required by Monitor)
- `tools` Command line tools such as LoadGenerator
- `data` Collected data
- `docs` Documentation

## Documentation
- [项目说明](docs/project.md)
- [流程(旧)](docs/流程.docx)

### Components
  - saasi (web interface)
  - CPU intensive microservice
  - IO intensive microservice
  - Memory intensive microservice
  - Business microservice (calls the above 3)
    - has 30 variants
  - Monitor
  - DM

## Automation

See [README](automation/README.md)
