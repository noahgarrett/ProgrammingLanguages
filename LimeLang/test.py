import requests
import time

st = time.time()

j = 1
for i in range(10000):
    j += i

print(f"Executed: {time.time() - st}")