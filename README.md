# PaymentGateway

## Overview

In this project, we implemented a system to provide online payment processing for credit card transactions. A brief disclaimer, though: by no means do we intend to process real-world payments. This project serves mainly as a learning tool. Right, let's dive into what makes processing a payment challenging and how we dealt with it here. First, we must understand where we stand in the payment process as a whole. When a person performs a purchase using a credit card, the retailer must somehow communicate with the bank that emitted the card to forward the task of validating and processing this payment. The task is not forwarded directly to the bank, though. The payment goes through a payment network first, such as Visa or Mastercard and than gets forwarded to the subject bank. We can see the payment networks as an abstraction layer over banks. With this decoupling between retailers and banks, the retailer does not have to know how to interact with each and every bank. He just needs to know how to interact with the payment networks he works with. That is great, but it turns out that this task still offers great challenges and workload that goes completely out of the scope of the retailer. That is where we enter. We offer integration between retailers and payment networks; thus, the retailer only needs to know how to communicate with us, and we are responsible for ensuring no payment gets lost. Before we go any deeper, a quick remark: another cool trait of having this payment network abstraction layer, from the customer's point of view now, is that we are not bounded to a geographical place or currency. We can be in Shanghai and use a credit card emitted in the US, and the payment network will take care of making the bridge between one side and the other. Alright, enough talk; let's get to the meat of the matter. *Why is it so challenging to build a system to connect with the network providers?* We must mention here PCI and other regulatory compliances and maintaining integrations in a dynamic industry. However, we want to focus on the architectural aspects of this challenge; we are not dealing with one client and one server processing a request. We must picture thousands or even millions of requests being dealt with daily. On such a high-scale distributed system, it is the same story we all know: servers will crash, networks will collapse, children will cry. So we need retrying mechanisms and idempotency everywhere. In such a complex and dynamic scenario with many responsibilities, we will surely use microservices. The question now is to queue or not to queue? Suppose we build our system using RESTful APIs to gather data from retailers and connect with the network providers. In that case, all the retrying logic will be on our servers' shoulders (adding complexity and not really attending to the SRP), we will be more prone to network partition on the communication between all our microservices and less reliable on surges and peak scenarios. Queues, on the other hand, abstract the retrying logic for us, give us a cushion for peak scenarios and decouple our services (among other things, such as the possibility of using topics to process stages of or pipeline in parallel, for example). Synchronous communication could be more suitable on a simpler system or one in which providing synchronous responses were mandatory. Below is a sketch of how we designed the system using queue-based communication microservices: 

<p align="center">
  <img width="750" height="750" src="https://github.com/LuisDoin/PaymentGateway/blob/master/Blob/PaymentGatewaySystemDesign.png">
</p>


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
```


