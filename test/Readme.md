# Running the e2e tests on a local machine
This gives you the most feedback because a chromium window will open and as you will see what is going on during the run.
1. Edit `origam\test\tests_e2e\additionalConfig.js` you probably want to change `backEndUrl` and the sql server connection settings
2. Edit `origam\test\tests_e2e\dbTools.js` and change path to `origam-utils.dll` to where you have it
   `const script = "dotnet ../origam-utils.dll test-db -a 1 -d 0 -c  \"EXEC " + procedureName + "\"";`
   for example
   `const script = "dotnet C:\\Repos\\origam\\backend\\origam-utils\\bin\\Debug\\net8.0\\origam-utils.dll test-db -a 1 -d 0 -c  \"EXEC " + procedureName + "\"";`
3. Edit `origam\test\tests_e2e\package.json` and remove `xvfb-run --auto-servernum` from the line
   `"test:e2e": "xvfb-run --auto-servernum jest --runInBand --config jest.config.js"`
4. If you only want to run a single test case (let's say it is "called test case 1"), edit `package.json` and add 
   `--t \"called test case 1\"` to the "test:e2e" command from the previous point
5. Open cmd in `origam\test\tests_e2e` and run `yarn test:2e2`


# Running the e2e tests in docker on a local machine
This is closer to the github action and can help you debug problems that cannot be repeated with the first approach. 
An advantage over just running the github action is that you can inspect files in the docker container after the tests
have finished. 
1. Copy the folder `origam/model-tests` to `origam/test` and rename it to `model` 
2. Create folder `origam/tests/HTML5`
3. Build server and copy contents of the bin folder `origam\backend\Origam.Server\bin\Debug\net8.0` to `origam/tests/HTML5`
4. Build frontend application and copy contents of the folder `origam\frontend-html\dist` to `origam\test\HTML5\clients\origam`
5. Open cmd in `origam/tests` and run `docker-compose --env-file envFile.env -f "docker-compose.yml" --profile test up`
    this will build the images, run the containers and the tests in `test_server` container
6. After the tests are finished you can get screenshots if any were taken with this command `docker cp test_server_1:/home/origam/HTML5/screenshots c://SomeFolder`
7. To rebuild the images first run `docker-compose --env-file envFile.env -f "docker-compose.yml" --profile test down` 
   then open the docker GUI and delete the images `test_server` and `test_databasesql`.
   After that you can run `docker-compose --env-file envFile.env -f "docker-compose.yml" --profile test up`.
    