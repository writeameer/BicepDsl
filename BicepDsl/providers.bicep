resource azureOpenAI 'orka.provider/openai.api' = {
  name: 'azure_openai'
  properties: {
    config: {
      endpoint: 'https://nprodopenai.openai.azure.com/'
      api_key: 'xxx'
      chat_deployment: 'gpt-4o'
      embedding_deployment: 'text-embedding-3-large'
    }
  }
}

resource mcpWeather 'orka.provider/mcp' = {
  name: 'mcp.weather'
  properties: {
    config: {
      endpoint: 'https://mcp.so/server/weather-mcp-server/TuanKiri'
      api_key: 'xxx'
    }
  }
}

