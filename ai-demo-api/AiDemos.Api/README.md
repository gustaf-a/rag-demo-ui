
# Rag Demo API

## Generating NSwag Client

Run the solution.

In for example Package Manager Console, run this:
nswag run nswag.json


## Setup

### Initialize the database with pgvector
    //TODO Initialize DB? Or add to readme. 4
    CREATE EXTENSION IF NOT EXISTS vector CASCADE;
    CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE;

#### Using docker


#### On Azure
Setup Parameters Extension Vector in Azure
   azure.extensions enable VECTOR.

   Then run   
    CREATE EXTENSION IF NOT EXISTS vector CASCADE;
