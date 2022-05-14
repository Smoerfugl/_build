# _build
## Environments supported

Set on execution --environment=${env}

```cs
Development,
Staging,
Production
```

## Pipeline.yml example
```yml
registry: registry.url
name: projectName
services:
- name: serviceA
    project: ServiceA
    dockerfile: Dockerfile
    servicePort: 3000
    replicas: 1
    hostname: service #if --domain=foo.bar is set on execution will be set to service.foo.bar
  - name: serviceB
    dockerfile: Dockerfile
    replicas: 1
    servicePort: 3000
    project: ServiceB
    
environmentVariables:
  production:
     - name: SomeVariable
       value: SomeValue 
     - name: SecondVariable #will be fetched from environment_variable upon execution
```

## Commands
```sh
_build kubernetes ingress
```
