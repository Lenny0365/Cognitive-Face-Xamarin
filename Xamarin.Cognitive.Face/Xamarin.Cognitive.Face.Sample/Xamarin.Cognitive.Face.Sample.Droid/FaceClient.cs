﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Java.Util;
using Xamarin.Cognitive.Face.Droid;
using Xamarin.Cognitive.Face.Sample.Droid.Extensions;
using Xamarin.Cognitive.Face.Sample.Shared;

namespace Xamarin.Cognitive.Face.Sample
{
	public partial class FaceClient
	{
		FaceServiceRestClient client;
		FaceServiceRestClient Client => client ?? (client = new FaceServiceRestClient (SubscriptionKey));

		FaceClient () { }


		#region Person Group


		public Task<List<PersonGroup>> GetPersonGroups (bool forceRefresh = false)
		{
			return Task.Run (() =>
			{
				try
				{
					if (Groups.Count == 0 || forceRefresh)
					{
						var groups = Client.ListPersonGroups ();

						Groups = new List<PersonGroup> (
							groups.Select (g => new PersonGroup
							{
								Id = g.PersonGroupId,
								Name = g.Name,
								UserData = g.UserData
							})
						);
					}

					return Groups;
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task<PersonGroup> GetPersonGroup (string personGroupId)
		{
			return Task.Run (() =>
			{
				try
				{
					var personGroup = Client.GetPersonGroup (personGroupId);

					return personGroup.ToPersonGroup ();
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task<PersonGroup> CreatePersonGroup (string groupName, string userData = null)
		{
			return Task.Run (() =>
			{
				try
				{
					var personGroupId = Guid.NewGuid ().ToString ();

					Client.CreatePersonGroup (personGroupId, groupName, userData);

					var personGroup = new PersonGroup
					{
						Name = groupName,
						Id = personGroupId,
						UserData = userData
					};

					Groups.Add (personGroup);

					return personGroup;
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task UpdatePersonGroup (PersonGroup personGroup, string groupName, string userData = null)
		{
			return Task.Run (() =>
			{
				try
				{
					Client.UpdatePersonGroup (personGroup.Id, groupName, userData);

					personGroup.Name = groupName;
					personGroup.UserData = userData;
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task DeletePersonGroup (string personGroupId)
		{
			return Task.Run (() =>
			{
				try
				{
					Client.DeletePersonGroup (personGroupId);

					RemoveGroup (personGroupId);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task TrainPersonGroup (PersonGroup personGroup)
		{
			return Task.Run (() =>
			{
				try
				{
					Client.TrainPersonGroup (personGroup.Id);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		/// <summary>
		/// Gets the group training status: notstarted, running, succeeded, failed
		/// </summary>
		/// <returns>The group training status.</returns>
		/// <param name="personGroupId">Person group Id.</param>
		public Task<TrainingStatus> GetGroupTrainingStatus (string personGroupId)
		{
			return Task.Run (() =>
			{
				try
				{
					var trainingStatus = Client.GetPersonGroupTrainingStatus (personGroupId);

					return trainingStatus.ToTrainingStatus ();
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		#endregion


		#region Person


		public Task<List<Person>> GetPeopleForGroup (PersonGroup personGroup, bool forceRefresh = false)
		{
			if (personGroup.People?.Count > 0 && !forceRefresh)
			{
				return Task.FromResult (personGroup.People);
			}

			return Task.Run (() =>
			{
				try
				{
					var arrPeople = Client.ListPersons (personGroup.Id);

					var people = new List<Person> (
						arrPeople.Select (p => p.ToPerson ())
					);

					if (personGroup.People != null)
					{
						personGroup.People.Clear ();
						personGroup.People.AddRange (people);
					}
					else
					{
						personGroup.People = people;
					}

					return people;
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task<Person> CreatePerson (string personName, PersonGroup personGroup, string userData = null)
		{
			return Task.Run (() =>
			{
				try
				{
					var result = Client.CreatePerson (personGroup.Id, personName, userData);

					var id = result.PersonId.ToString ();

					if (string.IsNullOrEmpty (id))
					{
						throw new Exception ("CreatePersonResult returned invalid person Id");
					}

					var person = new Person
					{
						Name = personName,
						Id = id,
						UserData = userData
					};

					personGroup.People.Add (person);

					return person;
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task UpdatePerson (Person person, PersonGroup group, string personName, string userData = null)
		{
			return Task.Run (() =>
			{
				try
				{
					Client.UpdatePerson (group.Id, UUID.FromString (person.Id), personName, userData);

					person.Name = personName;
					person.UserData = userData;
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		#endregion



		public Task<Face.Droid.Contract.Face []> Detect (MemoryStream stream, bool returnFaceId, bool returnLandmarks, FaceServiceClientFaceAttributeType [] attributes)
		{
			return Task.Run (() =>
			 {
				 return Client.Detect (stream, true, true, attributes);

			 });
		}

		public Task<Face.Droid.Contract.VerifyResult> Verify (UUID mFaceId0, UUID mFaceId1)
		{
			return Task.Run (() =>
			 {
				 return Client.Verify (mFaceId0, mFaceId1);
			 });
		}

		public Task<Face.Droid.Contract.VerifyResult> Verify (UUID mFaceId, String mPersonGroupId, UUID mPersonId)
		{
			return Task.Run (() =>
			 {
				 return Client.Verify (mFaceId, mPersonGroupId, mPersonId);
			 });
		}

		public Task DeletePerson (string mPersonGroupId, UUID mPersonId)
		{
			return Task.Run (() =>
			 {
				 Client.DeletePerson (mPersonGroupId, mPersonId);
			 });
		}

		public Task TrainPersonGroup (string mPersonGroupId)
		{
			return Task.Run (() =>
			 {
				 Client.TrainPersonGroup (mPersonGroupId);
			 });
		}

		public Task DeletePersonFace (string mPersonGroupId, UUID mPersonId, UUID mFaceId)
		{
			return Task.Run (() =>
			 {
				 Client.DeletePersonFace (mPersonGroupId, mPersonId, mFaceId);
			 });
		}

		public Task<Face.Droid.Contract.AddPersistedFaceResult> AddPersonFace (string mPersonGroupId, UUID mPersonId, Stream mImageStream, string userData, Face.Droid.Contract.FaceRectangle targetFace)
		{
			return Task.Run (() =>
			 {
				 return Client.AddPersonFace (mPersonGroupId, mPersonId, mImageStream, userData, targetFace);
			 });
		}

		public Task<Face.Droid.Contract.GroupResult> Group (UUID [] faceIds)
		{
			return Task.Run (() =>
			 {
				 return Client.Group (faceIds);
			 });
		}
	}
}