terraform {
  required_providers {
    null = {
      source = "hashicorp/null"
    }
  }
}

provider "null" {}

resource "null_resource" "example" {
  provisioner "local-exec" {
    command = "echo ${var.input_var}"
  }
}


variable "input_var" {
  description = "An input variable"
  type        = string
  default = "QUALQUER COISA AQUI"
}

output "output_var" {
  value = var.input_var
}
