# PaymentGateway

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

![ScreenShot](https://drive.google.com/uc?export=view&id=1IT512WfYeML-wminA4pgDOh9j5yKD563)

Select Properties.

![ScreenShot](https://drive.google.com/uc?export=view&id=1ZVkZOqBvv55DO5bfeZMVbs_DocctZyJU)

Toggle the 'Multiple startup projects' option and select 'Start' for the projects CKOBankSimulator, PaymentGateway
PaymentProcessor and TransactionsApi, as shown in the figure below.

![ScreenShot](https://drive.google.com/uc?export=view&id=1PDqAChBJOol--WowZ3VVB3DJbiil-pTv)

Click on the Start button.

![ScreenShot](https://drive.google.com/uc?export=view&id=1dxm7SPR8mLcg8hzwcKdEe5k_Ax4g3MtR)

This will open Swagger for our Payment Gateway, which we will use to test our system. 

![ScreenShot](https://drive.google.com/uc?export=view&id=1hq_-6FZLNvIws6PSnzXXt7qVocauakE1)

## Testing

The first step is to get a JWT on the Authentication endpoint. There are two users registered: Amazon and Nike. Their passwords are AWSSecret1, AWSSecret2, NikeSecret1 and NikeSecret2. Secrets '1' provides a Tier1 role with full access, while Secrets '2' provides a Tier2 role with access only to the Get endpoints.
With our JWT, we can unlock all the endpoints using the Authorize button at the top right.

![ScreenShot](https://drive.google.com/uc?export=view&id=1UyBZwr1iq18JX3KfNwfHFzTqZrSnF7hP)

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


![ScreenShot](https://drive.google.com/uc?export=view&id=1ynG96LFrhALI-m18WK2wB5cyLq_COqe_)


Calling the GET payments to check the status 'Processing'. Here we can also see the masked credit card number. 


![ScreenShot](https://drive.google.com/uc?export=view&id=1we3tFGJD1AaUpWWGoTj2D9s7MvebNOQx)


Check the return of the POST method to confirm we are observing the payment we have just posted.


![ScreenShot](https://drive.google.com/uc?export=view&id=1SOrKwCyGPM4zY7MygF630Abydwl_YLc-)


Call again the Get payments to verify the newly updated status.


![ScreenShot](https://drive.google.com/uc?export=view&id=1JIQnrLwpURc1d-fFCk-vcindHgI85K7i)


And we can accttually use this paymentId to test our GET payment endpoint.


![ScreenShot](https://drive.google.com/uc?export=view&id=1KRXFiPATdNxwmgrTQOAMzC_dEXdteLIi)


It would also make sense to test unsuccessful payments and perform further testing on the GET payments method, passing different values to the 'from' and 'to' parameters. 

As a final remark for this section, for our client to receive the final status of a payment we could apply two strategies: the client can perform a pooling to check payment status or alternatevely we could use a webhook so we can send a response to the client as soon as we are done processing the payment. 
```


