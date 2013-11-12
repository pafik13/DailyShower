using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using PresenterPlanner.Lib;
using PresenterPlanner.Lib.Doctors;
using PresenterPlanner.Lib.Hospitals;

namespace com.xamarin.sample.fragments.honeycomb
{
    public class TitlesFragment : ListFragment
    {
        private int _currentPlayId;
        private bool _isDualPane;
		protected List<string> docs;
		protected List<Doctor> chdocs;
		protected List <Presentation> presents;

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

			Setts sett = Common.GetSettings ();
			DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
			Calendar cal = dfi.Calendar;
			int week = (cal.GetWeekOfYear (DateTime.Today, dfi.CalendarWeekRule, dfi.FirstDayOfWeek) - sett.weekOfStart)%3;

			var choosenHospitals = HospitalManager.GetChoosenHospitals(week, DateTime.Today.DayOfWeek);

			var doctors = DoctorManager.GetDoctors ();

			docs = new List<string> ();
			chdocs = new List<Doctor> ();

			for (int d = 0; d < doctors.Count; d++) {
				for (int h = 0; h < choosenHospitals.Count; h++) {
					if (doctors [d].HospitalID == choosenHospitals [h].ID) {
						chdocs.Add (doctors [d]);
						docs.Add(doctors [d].FirstName + ' ' + doctors [d].SecondName + ' ' + doctors [d].ThirdName);
					}
				}
			}

            var detailsFrame = Activity.FindViewById<View>(Resource.Id.details);
            _isDualPane = detailsFrame != null && detailsFrame.Visibility == ViewStates.Visible;

            var adapter = new ArrayAdapter<String>(Activity, Android.Resource.Layout.SimpleListItemChecked, docs/*Shakespeare.Titles*/);
            ListAdapter = adapter;

			//progress.Dismiss();

            if (savedInstanceState != null)
            {
				_currentPlayId = savedInstanceState.GetInt("current_play_id", chdocs[0].ID);
            }

            if (_isDualPane)
            {
                ListView.ChoiceMode = ChoiceMode.Single;
				ShowDetails(chdocs[_currentPlayId].ID);
            }

			/////////////////////////////////////////////////////////////////////////////////
			if (presents == null) {
				var progress = ProgressDialog.Show (Activity, "Loading prsentations", "Please Wait (about 15 seconds)", true); 

				new Thread (new ThreadStart (() => {
					Thread.Sleep (15 * 1000);
					Activity.RunOnUiThread (() => {
						presents = Presentations.GetPresentations ();
						progress.Dismiss ();
					});
				})).Start ();
			}
			/////////////////////////////////////////////////////////////////////////////////

        }

        public override void OnListItemClick(ListView l, View v, int position, long id)
        {
            ShowDetails(chdocs[position].ID);
			// We can display everything in-place with fragments.
			// Have the list highlight this item and show the data.
			ListView.SetItemChecked(position, true);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutInt("current_play_id", _currentPlayId);
        }

        private void ShowDetails(int playId)
        {
            _currentPlayId = playId;
            if (_isDualPane)
            {

                // Check what fragment is shown, replace if needed.
                var details = FragmentManager.FindFragmentById(Resource.Id.details) as DetailsFragment;
                if (details == null || details.ShownPlayId != playId)
                {
                    // Make new fragment to show this selection.
                    details = DetailsFragment.NewInstance(playId);

                    // Execute a transaction, replacing any existing
                    // fragment with this one inside the frame.
                    var ft = FragmentManager.BeginTransaction();
                    ft.Replace(Resource.Id.details, details);
                    ft.SetTransition(FragmentTransit.FragmentFade);
                    ft.Commit();
                }
            }
            else
            {
                // Otherwise we need to launch a new activity to display
                // the dialog fragment with selected text.
                var intent = new Intent();

                intent.SetClass(Activity, typeof(DetailsActivity));
                intent.PutExtra("current_play_id", playId);
                StartActivity(intent);
            }
        }
    }
}
