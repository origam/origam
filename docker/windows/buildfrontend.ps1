cd c:\home\origam\HTML5-SOURCE
$Env:NODE_OPTIONS = "--max-old-space-size=2536"
yarn cache clean
yarn install --frozen-lockfile
yarn build