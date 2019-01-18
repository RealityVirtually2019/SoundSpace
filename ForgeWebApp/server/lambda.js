'use strict'
const awsServerlessExpress = require('aws-serverless-express')
const server = awsServerlessExpress.createServer(require('./server'))
exports.handler = (event, context) => awsServerlessExpress.proxy(server, event, context)