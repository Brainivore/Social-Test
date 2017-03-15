using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;

public class Test : MonoBehaviour {
	public GameObject LoggedInUI;
	public GameObject NotLoggedInUI;
	public GameObject Friend;

	void Awake(){
		if (!FB.IsInitialized) {
			FB.Init (InitCallBack);
		}
	}

	void InitCallBack(){
		Debug.Log ("FB has been initiased.");
		ShowUI ();
	}

	public void Login(){
		if (!FB.IsLoggedIn) {
			FB.LogInWithReadPermissions (new List<string>{ "user_friends" }, LoginCallBack);
		}
	}

	void LoginCallBack(ILoginResult result){
		if (result.Error == null) {
			Debug.Log ("FB has logged in.");
			ShowUI ();
		} else {
			Debug.Log ("Error during login: " + result.Error);
		}
	}

	void ShowUI(){
		if (FB.IsLoggedIn) {
			LoggedInUI.SetActive (true);
			NotLoggedInUI.SetActive (false);
			FB.API ("me/picture?width=100&height=100", HttpMethod.GET, PictureCallBack);
			FB.API ("me?fields=first_name", HttpMethod.GET, NameCallBack);
			FB.API ("me/friends", HttpMethod.GET, FriendCallBack);
		} else {
			LoggedInUI.SetActive (false);
			NotLoggedInUI.SetActive (true);
		}
	}

	void PictureCallBack(IGraphResult result){
		Texture2D image = result.Texture;
		LoggedInUI.transform.FindChild ("ProfilePicture").GetComponent<Image> ().sprite = Sprite.Create (image, new Rect (0, 0, 100, 100), new Vector2 (0.5f, 0.5f));
	}

	void NameCallBack(IGraphResult result){
		IDictionary<string, object> profile = result.ResultDictionary;
		LoggedInUI.transform.FindChild ("Name").GetComponent<Text> ().text = "Hello " + profile ["first_name"];
	}

	public void LogOut(){
		FB.LogOut ();
		ShowUI ();
	}

	public void Share(){
		FB.ShareLink (new System.Uri("http://brainivore.com"), "This game is awesome!", "A description of the game.", new System.Uri("http://brainivore.com/Images/Logo.png"));
	}

	public void Invite(){
		FB.AppRequest (message: "You should really try this game.", title: "Check this super game!");
	}

	void FriendCallBack(IGraphResult result){
		IDictionary<string, object> data = result.ResultDictionary;
		List<object> friends = (List<object>)data ["data"];
		foreach (object obj in friends) {
			Dictionary<string, object> dictio = (Dictionary<string, object>)obj;
			CreateFriend(dictio ["name"].ToString (), dictio ["id"].ToString ());
		}
	}

	void CreateFriend(string name, string id){
		GameObject myFriend = Instantiate (Friend);
		Transform parent = LoggedInUI.transform.FindChild ("ListContainer").FindChild ("FriendList");
		myFriend.transform.SetParent (parent);
		myFriend.GetComponentInChildren<Text> ().text = name;
		FB.API(id + "/picture?width=100&height=100", HttpMethod.GET, delegate(IGraphResult result) {
			myFriend.GetComponentInChildren<Image>().sprite = Sprite.Create (result.Texture, new Rect (0, 0, 100, 100), new Vector2 (0.5f, 0.5f));
		});
	}
}

