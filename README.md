# workflow-camunda-sample

An example of using the camunda platfrom to control the process with kafka

![image](https://github.com/user-attachments/assets/6a019fac-a810-4fed-aa99-1fad2e04855c)


![image](https://github.com/user-attachments/assets/7b98b515-1df0-415a-acee-0702c6dd4322)

![image](https://github.com/user-attachments/assets/9360440f-79ab-477f-a4b6-363be6d989ac)

![image](https://github.com/user-attachments/assets/b0805838-2ca2-4578-9d88-82662c987085)


## Application modules
![image](./img/Modules.png)

## Tracing
![image](./img/Traces.png)

## to run
* configure the `Aspire.AppHost` [appsetings.json](Modules/Aspire/Aspire/Aspire.AppHost/appsettings.json) profile. 
* run `Aspire.AppHost`

## external depedencies
* camunda-startup
	* https://github.com/rafalkopryk/CamundaStartup
	* update command `git subtree pull --prefix external/camunda-startup camunda-startup master --squash`
