import requests, json

CLIENT_ID = "9812fcc251af4115a42369c5ea1fc8ac"
CLIENT_SECRET = "8ffbb87f46bc48ceb193e40478559915"

grant_type = 'client_credentials'
body_params = {'grant_type' : grant_type}

url='https://accounts.spotify.com/api/token'
response = requests.post(url, data=body_params, auth = (CLIENT_ID, CLIENT_SECRET))

token_raw = json.loads(response.text)
token = token_raw["access_token"]

headers = {"Authorization": "Bearer {}".format(token)}


r = requests.get(url="https://api.spotify.com/v1/search?q=tupac&type=artist", headers=headers)
print(r.text)