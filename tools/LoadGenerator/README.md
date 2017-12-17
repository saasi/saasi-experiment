# Use LoadGenerator with Docker

## First Build
```bash
sudo chmod +x ./build.sh
./build.sh
```

## Then Run
```bash
sudo chmod +x ./run.sh
./run.sh <type> <usercount> <requestTime>
```

For example:

To run experiment for Evaluation 3, with 60 concurrent users, and sending requests in 3 batches, run:

```bash
./run.sh 3 60 3
```

The output data is located in the `data` folder. *Do not* check-in the data to the repository. (Unless you really mean to.)