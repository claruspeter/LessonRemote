
terraform {   
    backend "azurerm" {     
        resource_group_name   = "Default-Storage-WestUS"
        storage_account_name  = "appstates"     
        container_name        = "tfstate"     
        key                   = "remotelesson.tfstate"   
    } 
}
variable "subscription_id" {
    type = string
}

variable "tenant_id" {
    type = string
}

variable "app_name" {
    type = string
}