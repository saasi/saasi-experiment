## SmartVM experiments

## Folder structure
- `controlvm` Code to run on the **master** node
  - `globalMonitor`
  - `tools`
- `workvm` Code to run on the **slave** nodes
  - `Application1`
  - `Application2`
  - `Application3`
- `data` Collected data
- `docs` Documentation

## Documentation
- [项目说明](docs/项目说明.docx)
- [流程](docs/流程.docx)

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
