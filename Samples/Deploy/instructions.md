# Deploy Sample Contents

1. Please check [`deploy`] command to learn how to use deploy commands.
2. Ensure your [Service Account] have the project roles for [Deploy Command].
3. [Login] CLI with service account.
4. Set [configuration] for your project-id and environment.
5. Now follow the instructions below to deploy for different services.

[Deploy Cloud Code Script](#deploy-cloud-code-script)<br>
[Deploy Remote Config](#deploy-remote-config)<br>

## Deploy Cloud Code Script

Run command from [Samples/Deploy] directory:
```
ugs deploy ./CloudCode
```
You will find [Script.js] published in your dashboard for the configured project and environment. The command deploys supported contents in `CloudCode` directory. 

### Create Cloud Code Script

To create a deployable cloud code script, you need `module.exports` and `module.exports.params` in your script:

```JS
module.exports = async ({ params, context, logger }) => {
  // Please define your script logic
}

// Please define your script parameters
module.exports.params = {
    range: "NUMERIC"
};
```
Please take [Script.js] as an example. For more details, please check [Declare parameters in the script].

## Deploy Remote Config

Run command from [Samples/Deploy] directory:
```
ugs deploy ./RemoteConfig
```
You will find all the keys in [configuration.rc] published in your dashboard for the configured project and environment. The command deploys support contents in `RemoteConfig` directory.

### Create Remote Config Files:

To create a deployable remote config file, you need a `.rc` file with the following pattern:
```Json
{
  "$schema": "https://ugs-config-schemas.unity3d.com/v1/remote-config.schema.json",
  "entries": {
    "your-unique-config-key": 100.2
  }
  "types" : {
    "your-unique-config-key": "FLOAT"
  }
}
```
Please take [configuration.rc] as an example. For more details, please check [Remote Config files].

## Deploy all Samples
Run command from [Samples/Deploy] directory:
```
ugs deploy .
```
You will find all the contents deployed in your dashboard for the configured project and environment. The command deploys all the samples in current (`Deploy`) directory for supported services.

---
[`deploy`]: /docs/deploy-command.md
[Remote Config files]: https://docs.unity3d.com/Packages/com.unity.remote-config@3.3/manual/Authoring/remote_config_files.html
[Declare parameters in the script]: https://docs.unity.com/cloud-code/authoring-scripts-editor.html#Declare_parameters_in_the_script
[Script.js]: /Samples/Deploy/CloudCode/Script.js
[configuration.rc]: /Samples/Deploy/RemoteConfig/configuration.rc
[Samples/Deploy]: /Samples/Deploy
[Deploy Command]: /docs/project-roles.md#deploy-command
[Service Account]: https://services.docs.unity.com/docs/service-account-auth/index.html
[Login]: /docs/module-authentication.md#login
[configuration]: /docs/module-configuration.md#configuration-module