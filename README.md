# PaymentGateway

## Overview

In this project, we implemented a system to provide online payment processing for credit card transactions. A brief disclaimer, though: by no means do we intend to process real-world payments. This project serves mainly as a learning tool. Right, let's dive into what makes processing a payment challenging and how we dealt with it here. First, we must understand where we stand in the payment process as a whole. When a person performs a purchase using a credit card, the retailer must somehow communicate with the bank that emitted the card to forward the task of validating and processing this payment. The task is not forwarded directly to the bank, though. The payment goes through a payment network first, such as Visa or Mastercard and than gets forwarded to the subject bank. We can see the payment networks as an abstraction layer over banks. With this decoupling between retailers and banks, the retailer does not have to know how to interact with each and every bank. He just needs to know how to interact with the payment networks he works with. That is great, but it turns out that this task still offers great challenges and workload that goes completely out of the scope of the retailer. That is where we enter. We offer integration between retailers and payment networks; thus, the retailer only needs to know how to communicate with us, and we are responsible for ensuring no payment gets lost. Before we go any deeper, a quick remark: another cool trait of having this payment network abstraction layer, from the customer's point of view now, is that we are not bounded to a geographical place or currency. We can be in Shanghai and use a credit card emitted in the US, and the payment network will take care of making the bridge between one side and the other. Alright, enough talk; let's get to the meat of the matter. *Why is it so challenging to build a system to connect with the payment networks?* We must mention here PCI and other regulatory compliances and maintaining integrations in a dynamic industry. However, we want to focus on the architectural aspects of this challenge; we are not dealing with one client and one server processing a request. We must picture thousands or even millions of requests being dealt with daily. On such a high-scale distributed system, it is the same story we all know: servers will crash, networks will collapse, children will cry. So we need retrying mechanisms and idempotency everywhere. In such a complex and dynamic scenario with many responsibilities, we will surely use microservices. The question now is to queue or not to queue? Suppose we build our system using RESTful APIs to gather data from retailers and connect with the payment networks. In that case, all the retrying logic will be on our servers' shoulders (adding complexity and not really attending to the SRP), we will be more prone to network partition on the communication between all our microservices and less reliable on surges and peak scenarios. Queues, on the other hand, abstract the retrying logic for us, give us a cushion for peak scenarios and decouple our services (among other things, such as the possibility of using topics to process stages of or pipeline in parallel, for example). Synchronous communication could be more suitable on a simpler system or one in which providing synchronous responses were mandatory. Below is a sketch of how we designed the system using queue-based communication microservices: 

<p align="center">
  <img width="750" height="750" src="https://github.com/LuisDoin/PaymentGateway/blob/master/Blob/PaymentGatewaySystemDesign.png">
</p>

## Services Description
### Payment Gateway

This is the entry point of our system, where we expose endpoints for retailers to connect. We provide an endpoint for processing a new payment and two GET methods; one for fetching a payment based on its id an another for fetching all payments one retailer has processed in a specified period. There are a couple of interesting things to note about this service. The first is that it is responsible for generating a paymentId for each payment. As soon as a payment enters our system, it receives a birth certificate, so to speak. Generating the paymentId will allow us to achieve idempotency later in our pipeline. This will become clearer when we analyze the Payment Processor service. Another interesting point about the Payment Gateway service is that as soon as it creates a paymentId, it calls the Transactions service, which is responsible for maintaining our paymentsDB (more on that later). This payment is inserted with a 'Processing' status. This is one of the three possible statuses, the others being successful and unsuccessful. This status will be updated once the payment has been processed. In this manner, we have access to the status of a given transaction at every point in time. One last thing to mention is that this service is responsible for validating any incoming request before feeding it to our pipeline. After which, it serves this message to the Pending Transaction Queue to be consumed by the Payment Processor service. 

### Payment Processor

This service has the main responsibility of integrating with the CKO Bank Simulator. It sends the bank payment information to be processed. Here we are simulation communication directly with a bank; in a real-world scenario, we would integrate with a payment network such as Visa or Mastercard. We mentioned that producing the paymentId on the Payment Gateway would be essential for achieving idempotency. Here is the reason: we must consume the bank's API idempotently, so if (when) we process a message more than once, we guarantee that the payment will be effectively processed only (and exactly) once. To achieve that, we send the bank the transactionId of each transaction. With it, the bank implements his logic never to process twice the same paymentId. Now, if we were to generate the paymentId on the Payment Processor service, we would generate one paymentId for each time we consume a message. Therefore if we were to consume one message twice, two different paymentIds would be generated and sent to the bank; thus, the bank would effectively process it multiple times. Ok, we went too far on the micros of the system's behavior, but it is nice to have this clear in our minds. Once the service receives the bank's response, it forwards it to the Completed Transactions Queue to be consumed by the Transactions service.

### Transactions

The main responsibility of this service is managing our PaymentDB. It saves and updates payments whenever they reach a new status (the Payment Gateway sends 'Processing' payments, and 'Successful and 'Unsuccessful' are consumed from the queue). We chose to use SQL Server since we need strong consistency and also intend to perform complex queries. For scaling, we can rely on sharding. A possible sharding strategy is shading based on retailers, grouping multiple retailers on a single shard depending on the retailer request volume and size. One challenge that this strategy presents that we must keep in mind is that retailers' size and request volume are dynamic. A few reshardings might be necessary along the way. Going back to our service responsibilities, it also provides endpoints for data fetching. In fact, what the Payment Gateway does with its GET endpoints is forward the request to the service in charge of it, the Transactions service. One last thing to mention about this service is that he plays a central role in allowing us to inform retailers when payments have been fully processed. This can happen via polling from the retailer (to our Payment Gateway) or webhooks retailers could have previously set up for which the Transactions service would proactively send requests (to the Payment Gateway, who would forward it to the retailers) to inform payment completion and status. 

### CKO Bank Simulator

This service is fairly simple. He receives payments to be processed, saves their Id to a Redis database (so all service instances can observe the same already-processed payments), and process them. We did not implement the processing logic since it was out of this project's scope. Another interesting point not properly implemented here (again due to our scope) was making this service idempotent. He sure appears to be idempotent since we insert every processed id to Redis and enquires about it before processing any payment. But we have to think about the scenario where it processes the payment and, for example, the server crashes before saving it to Redis. If this payment ever arrives again (and it will since we employ retrying mechanisms), the bank will process it again. A more involved design is needed to fix this, such as using a state machine. Finally, it may be important to know that the bank simulates the available limit of a client by generating a random number from 50 to 100. So if we want, for testing purposes, for example, to make a request to generate a successful payment, we simply set any amount no greater than 50. Likewise, any amount greater than 100 will generate an unsuccessful transaction (given all other inputs are valid, of course). 

## API Documentation


```http
  POST /Authentication/login
```

| Parameter   | Type       | Description                           |
| :---------- | :--------- | :---------------------------------- |
| `login` | `string` | **Required** |
| `password` | `string` | **Required** |

#### Returns a JSON Web Token. There are two users registered: Amazon and Nike. Their passwords are AWSSecret1, AWSSecret2, NikeSecret1 and NikeSecret2. Secrets '1' provides a Tier1 role with full access, while Secrets '2' provides a Tier2 role with access only to the Get endpoints.

```http
  POST /Payments/payment
```

| Parameter   | Type       | Description                           |
| :---------- | :--------- | :---------------------------------- |
| `paymentDetails` | `PaymentDetails` | **Required** |
| `delayInSeconds` | `int` | **Optional** It's value is 0 by default. Used for testing. Further information can be found on the `Testing` section.|


| PaymentDetails's fields   | Type       | Description                           |
| :---------- | :--------- | :---------------------------------- |
| `CreditCardNumber` | `string` | **Required** Must be a vaild credit card number as explained [here](https://smallbusiness.chron.com/validate-credit-card-information-43910.html). |
| `ExpirationDate` | `string` | **Required** Must be in the format mm/yyyy.|
| `Cvv` | `string` | **Required** Must contain 3 or 4 digits.|
| `Currency` | `string` | **Required** List of supported currencies: USD, EUR, GBP, JPY, CNY, AUD, CAD, CHF, HKD, SGD.|
| `Amount` | `decimal` | **Required** Must be greater or equal to zero.|

`Valid credit card number for testing: 4324781866717289. Other valid number can be generated` [here](https://www.vccgenerator.org/).

#### Returns the posted payment's Id. 

```http
  GET /Payments/payment
```

| Parâmetro   | Tipo       | Descrição                                   |
| :---------- | :--------- | :------------------------------------------ |
| `id`      | `string` | **Required**. The id of the requested payment. |

#### Returns the requested payment if it exists. 

```http
  GET /Payments/payments
```

| Parâmetro   | Tipo       | Descrição                                   |
| :---------- | :--------- | :------------------------------------------ |
| `from`      | `DateTime` | **Required**|
| `to`      | `DateTime` | **Required**|

#### Returns all payment issued by the current user with last status update between from and to DateTimes. 

## Running locally

Clone the project

```bash
  git clone https://github.com/LuisDoin/PaymentGateway.git
```

Enter the project directory

```bash
  cd PaymentGateway
```

Raise infrastructures

```bash
  docker-compose up -d
```

Open the Solution using Visual Studio (2019 or 2022) and right-click on the Solution 'PaymentGateway'.

![image](/Blob/image1.png)

Select Properties.

![image](/Blob/image2.png)

Toggle the 'Multiple startup projects' option and select 'Start' for the projects CKOBankSimulator, PaymentGateway
PaymentProcessor and TransactionsApi, as shown in the figure below.

![image](/Blob/image3.png)

Click on the Start button.

![image](/Blob/image4.png)

This will open Swagger for our Payment Gateway, which we will use to test our system. 

![image](/Blob/image5.png)

## Testing

The first step is to get a JWT on the Authentication endpoint. There are two users registered: Amazon and Nike. Their passwords are AWSSecret1, AWSSecret2, NikeSecret1 and NikeSecret2. Secrets '1' provides a Tier1 role with full access, while Secrets '2' provides a Tier2 role with access only to the Get endpoints.
With our JWT, we can unlock all the endpoints using the Authorize button at the top right.

![image](/Blob/image6.png)

Ok, with authentication out of the way, let's test our API. The first endpoint that makes sense to test is the POST endpoint. Here we have added an optional parameter to aid us during testing. When we call this endpoint, the payment has the status 'Processing' (persisted on our PaymentsDb). Later this status is updated to either 'Successful' or 'Unsuccessful'. The issue is that this process happens so fast that we cannot catch it during testing. So we added the delayInSeconds parameters that will stop the process for the required amount of seconds right after the database insertion. In the meanwhile, we may call the GET payments endpoint (since we do not possess the paymentId yet) and see our payment on the top of the list with a 'Processing' status. When our POST endpoint finally returns, we can confirm that the returned paymentId and the paymentId returned by the GET payments return are the same. We may also call the GET payments endpoint again to check the newly updated status. Something to keep in mind is that the GET endpoint performs validations on every input and some of them are not intuitive, especially the ones made over the credit card number ([here](https://smallbusiness.chron.com/validate-credit-card-information-43910.html) we have a brief explanation on this). So here is an example of an object that passes all validation (we can generate more valid credit card numbers [here](https://www.vccgenerator.org/) if we want):

{
  "creditCardNumber": "4324781866717289",
  "expirationDate": "01/2020",
  "cvv": 111"",
  "currency": "USD",
  "amount": 10
}

This will generate a successful transaction. If we want to simulate an unsuccessful transaction, we may change the amount to 1000. This is because the CKOBankSimulator simulates the operations of checking how much available limit a client has by randomly picking any number from 50 to 100. So if the amount is less than 50, the transaction will be successful, and any amount greater than 100 will generate an unsuccessful transaction. And, given valid inputs, this is the only criterion that will determine the final status of a transaction since the code has no bugs that I know of, and our system's retries will deal with any possible transient issue.
Alright, below there are some pictures depicting the usage of the delayInSeconds parameter. 


Call POST method passing a 30 seconds delay.


![image](/Blob/image7.png)

Calling the GET payments to check the status 'Processing'. Here we can also see the masked credit card number. 


![image](/Blob/image8.png)


Check the return of the POST method to confirm we are observing the payment we have just posted.


![image](/Blob/image9.png)


Call again the Get payments to verify the newly updated status.


![image](/Blob/image10.png)


And we can accttually use this paymentId to test our GET payment endpoint.


![image](/Blob/image11.png)


It would also make sense to test unsuccessful payments and perform further testing on the GET payments method, passing different values to the 'from' and 'to' parameters. 

As a final remark for this section, for our client to receive the final status of a payment we could apply two strategies: the client can perform a pooling to check payment status or alternatevely we could use a webhook so we can send a response to the client as soon as we are done processing the payment. 

## Future Improvements

* Our Payment Gateway service is currently using mocked-in-memory data for user authentication. We plan to add a Retailer Profile Service for managing a RetailerDB to store retailers' registration data. 
* Add encryption to our transactionsDB.
* Improve code coverage of our unit tests.
* Add integration tests.
* Add functional tests.
* Add pagination to GET payments.

```


