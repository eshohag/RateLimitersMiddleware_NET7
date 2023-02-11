## Rate Limiting Middleware in ASP.NET Core (.NET7)

### Rate Limiter Algorithms
The RateLimiterOptionsExtensions class provides the following extension methods for rate limiting:

#### Fixed window
`Fixed window limit` lets you apply limits such as “60 requests per minute”. Every minute, 60 requests can be made. One every second, but also 60 in one go</br>

#### Sliding window
`Sliding window limit` is similar to the fixed window limit, but uses segments for more fine-grained limits. Think “60 requests per minute, with 1 request per second” </br>

#### Token bucket
`Token bucket limit` lets you control flow rate, and allows for bursts. Think “you are given 100 requests every minute”. If you make all of them over 10 seconds, you’ll have to wait for 1 minute before you are allowed more requests

#### Concurrency
`Concurrency limit` is the simplest form of rate limiting. It doesn’t look at time, just at number of concurrent requests. “Allow 10 concurrent requests”</br></br></br>
More details link https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
