**Steps to deploy an instance of safeexchange.cp:**

1. Create resource group **{GROUP NAME}** in Azure.
2. Fill out corresponding parameters in parameters.json.
3. Run **az cli** commands:

```
az login
az deployment group create --resource-group {GROUP NAME} --template-file ./deployment/arm/services-template.arm.json --parameters ./deployment/arm/services-parameters.arm.json
```
5. After successful deployment, deploy backend to newly created azure function:

```
az functionapp deployment source config-zip -g {GROUP NAME} -n {FUNCTION NAME} --src {PATH TO ZIP DEPLOYMENT FILE}
```
