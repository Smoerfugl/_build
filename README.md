# _build
## Environments supported

Set on execution --env=${env}

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
    project: ServiceA/ServiceA.csproj
    servicePort: 80
    replicas: 1
    hostname: service #if --domain=foo.bar is set on execution will be set to service.foo.bar
  - name: serviceB
    replicas: 1
    project: ServiceB/ServiceB.csproj
    
environmentVariables:
  production:
	- name: SomeVariable
	  value: SomeValue
	  
	- name: SecondVariable //will be fetched from environment_variable upon execution
```