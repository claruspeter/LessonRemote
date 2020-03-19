provider "azurerm" {
    version = "~>1.36.0"

    subscription_id = var.subscription_id
    tenant_id = var.tenant_id
}


resource "azurerm_resource_group" "rg" {
    name     = "${var.app_name}-rg"
    location = "westus"
}

resource "azurerm_app_service_plan" "plan" {
    name                = "${var.app_name}-plan"
    location            = azurerm_resource_group.rg.location
    resource_group_name = azurerm_resource_group.rg.name
    kind                = "Windows"

    sku {
        tier = "Free"
        size = "F1"
    }
}

resource "azurerm_app_service" "app" {
    name                = "${var.app_name}"
    location            = azurerm_resource_group.rg.location
    resource_group_name = azurerm_resource_group.rg.name
    app_service_plan_id = azurerm_app_service_plan.plan.id
    https_only          = true

    # storage_account {
    #     name            = "${var.app_name}_Files"
    #     type            = "AzureFiles"
    #     account_name    = azurerm_storage_account.storage.name
    #     share_name      = azurerm_storage_share.files.name
    #     access_key      = azurerm_storage_account.storage.primary_access_key
    #     mount_path      = "/files"
    # }

    # app_settings = {
    #     "DATA_FOLDER" = "/files"
    # }
}
