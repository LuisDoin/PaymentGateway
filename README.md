# PaymentGateway

## Rodando localmente

Clone o projeto

```bash
  git clone https://github.com/LuisDoin/PaymentGateway.git
```

Entre no diretório do projeto

```bash
  cd PaymentGateway
```

Raise de infrastructure

```bash
  docker-compose up -d
```

Inicie o servidor

Open the Solution using Visual Studio (2019 or 2022) and right-click on the Solution 'PaymentGateway'.

Select Properties.

Toggle the 'Multiple startup projects' option 
and select 'Start' for the projects CKOBankSimulator, PaymentGateway
PaymentProcessor, TransactionsApi, as shown in the figure.

Click on the Start button.
This will open swagger for our Payment Gateway, which we will be using to test out system. 
