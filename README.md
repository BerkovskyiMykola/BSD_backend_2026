# BSD_backend_2026

## Task Requirements

### Background Knowledge

To understand the task, educate yourself about the order book (basic knowledge is more than enough).
For simplicity, let's assume that we are dealing with a cryptoexchange that only offers Bitcoin (BTC) and you can sell or buy it only for EUR. At any given time, an order book is the bid-ask state of this cryptoexchange. In other words, an order book is a bunch of "bids" and "asks". In our example, a bid is the price at which the buyer is willing to buy a certain amount of BTC. An ask is the price at which the seller is willing to sell a certain amount of BTC. See Figure 1. When a bid and an ask are matched, a trade is made, but this is not relevant for solving this task.
Take a look at Figure 2. How much EUR do you need to buy 9 BTC at the lowest possible price? The answer is 7 x 3k EUR + 2 x 3.3k EUR = 27,600 EUR.

### The Task

#### The Part 1

Your task is to implement a meta-exchange that always gives the user the best possible price if he is buying or selling a certain amount of BTC. Technically, you will be given n order books [from n different cryptoexchanges], the type of order (buy or sell), and the amount of BTC that our user wants to buy or sell. Your algorithm needs to output one or more buy or sell orders that then our system called Hedger issues to one or more of these n cryptoexchanges. In effect, our user buys the specified amount of BTC for the lowest possible price, or sells the specified amount of BTC for the highest possible price.

To make life a bit more complicated, each cryptoexchange has EUR and BTC balance. Your algorithm needs to achieve the best price within these constraints. The algorithm cannot transfer any money and/or crypto between cryptoexchanges.
Attached to this e-mail, you will find a bunch of JSON files with order books. Use these to test your solution (there is one order book per line in the file, take as many as you need from there). Your solution should be a relatively simple .NET console application, which reads the order books, balance constraints, and order type / size, and outputs (to console) a set of orders to execute against the given order books (exchanges). Please prepare a function that solves the task.

#### The Part 2

Implement a Web service (Kestrel, .NET API), and expose the implemented functionality through it. Implement an endpoint that will receive the required parameters (the type of order and the amount of BTC that our user wants to buy or sell), and return a JSON response with the "best execution" plan. Please also provide SwaggerUI or provide some other means for easy API invocation.

### Bonus Task

Deploy your Web service locally with Docker.

## Implementation Approach

This solution is developed with the following principles in mind:

- **Assumption of Valid Data**: The implementation assumes that all input data will be valid. Error handling for invalid data formats or ranges is not included, as the focus is on solving the core business problem.
- **KISS (Keep It Simple, Stupid)**: The code is intentionally kept as simple as possible. Complex abstractions and unnecessary patterns are avoided in favor of clarity and directness.
- **YAGNI (You Aren't Gonna Need It)**: Only features that are explicitly required by the task are implemented. No speculative or "nice-to-have" functionality is added.
- **Focus on Business Logic**: The primary goal is to understand and correctly implement the business requirements, not to demonstrate advanced architectural patterns or techniques.

This approach ensures the solution remains focused on solving the core problem at hand while remaining easy to understand and maintain.

## Solution

### ConsoleApp

The application is configured via `appsettings.json`.

#### Structure

```json
{
  "Take": 3,
  "OrderType": "Buy",
  "TargetAmount": 4,
  "EurBalances": {
    "1": 10000,
    "2": 5000,
    "3": 2050
  },
  "BtcBalances": {
    "1": 5,
    "2": 4,
    "3": 3
  }
}
```

#### Settings

##### **Take**
- **Type:** `int`  
- **Description:** Limits how many order book levels are loaded from the data source.

##### **OrderType**
- **Type:** `string` (`Buy | Sell`)  
- **Description:**  
  - **Buy** – builds a buy execution plan.  
  - **Sell** – builds a sell execution plan using bids.

##### **TargetAmount**
- **Type:** `decimal`  
- **Description:** The total amount of BTC to execute in the order plan.

##### **EurBalances**
- **Type:** `dictionary<int, decimal>`  
- **Description:** Available EUR balance per cryptoexchange.  
  - **Keys** represent cryptoexchange identifiers.  
  - **Values** represent available EUR balance.

##### **BtcBalances**
- **Type:** `dictionary<int, decimal>`  
- **Description:** Available BTC balance per cryptoexchange.  
  - **Keys** represent cryptoexchange identifiers.  
  - **Values** represent available BTC balance.

### WebApplication

The application is configured via `appsettings.json`.

#### Structure

```json
{
  "Take": 3,
  "EurBalances": {
    "1": 10000,
    "2": 5000,
    "3": 2050
  },
  "BtcBalances": {
    "1": 5,
    "2": 4,
    "3": 3
  }
}
```

#### Settings

##### **Take**
- **Type:** `int`  
- **Description:** Limits how many order book levels are loaded from the data source.

##### **EurBalances**
- **Type:** `dictionary<int, decimal>`  
- **Description:** Available EUR balance per cryptoexchange.  
  - **Keys** represent cryptoexchange identifiers.  
  - **Values** represent available EUR balance.

##### **BtcBalances**
- **Type:** `dictionary<int, decimal>`  
- **Description:** Available BTC balance per cryptoexchange.  
  - **Keys** represent cryptoexchange identifiers.  
  - **Values** represent available BTC balance.

#### Endpoints

##### **POST /execution-plan**

Generates the best execution plan for buying or selling a specified amount of BTC across multiple cryptoexchanges.

**Request Body:**

```json
{
  "orderType": "buy",
  "targetAmount": 4
}
```

**Parameters:**
- **orderType** (`string`): `"buy"` or `"sell"` - The type of order to execute.
- **targetAmount** (`decimal`): The total amount of BTC to buy or sell.

**Example Request:**

```bash
curl --location 'http://localhost:5124/execution-plan' \
  --header 'Content-Type: application/json' \
  --data '{
    "orderType": "buy",
    "targetAmount": 4
  }'
```

**Response:**

Returns a JSON response containing the optimal execution plan with orders to execute against the specified exchanges.

