cd c:\home\origam\HTML5-SOURCE
$Env:NODE_OPTIONS = "--max-old-space-size=1536"
yarn install --frozen-lockfile
yarn build