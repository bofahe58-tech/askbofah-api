using Android.Content;
using AskBofah.Models;
using AskBofah.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntelliJ.Lang.Annotations;
using System.Collections.ObjectModel;

namespace AskBofah.ViewModels
{
    public partial class AdminViewModel : ObservableObject
    {
        private readonly ApiClient _api;

        public AdminViewModel(ApiClient api)
        {
            _api = api;
            _ = LoadAsync();
        }

        public ObservableCollection<AdminUser> Users { get; } = new();

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string countText = "0 users";

        // ============================================================
        // LOAD USERS
        // ============================================================
        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var list = await _api.GetAsync<List<AdminUser>>("admin/users");

                Users.Clear();
                if (list is not null)
                {
                    foreach (var u in list)
                        Users.Add(u);
                }

                CountText = $"{Users.Count} user{(Users.Count == 1 ? "" : "s")}";
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Load failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ============================================================
        // PROMOTE / DEMOTE
        // ============================================================
        [RelayCommand]
        private async Task ToggleAdminAsync(AdminUser? user)
        {
            if (user is null) return;

            var confirm = await Shell.Current.DisplayAlert(
                user.IsAdmin ? "Demote user" : "Promote user",
                $"{user.Email} → {(user.IsAdmin ? "User" : "Admin")}?",
                "Confirm", "Cancel");

            if (!confirm) return;

            try
            {
                var newRole = !user.IsAdmin;

                // The backend uses PATCH — but ApiClient doesn't have Patch yet.
                // We'll use PostAsync to a new endpoint we add below.
                await _api.PostAsync<object>(
                    $"admin/users/{user.Id}/role",
                    new { isAdmin = newRole });

                await LoadAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Update failed", ex.Message, "OK");
            }
        }

        // ============================================================
        // DELETE USER
        // ============================================================
        [RelayCommand]
        private async Task DeleteUserAsync(AdminUser? user)
        {
            if (user is null) return;

            var confirm = await Shell.Current.DisplayAlert(
                "Delete user",
                $"Delete {user.Email}? This cannot be undone.",
                "Delete", "Cancel");

            if (!confirm) return;

            try
            {
                await _api.DeleteAsync($"admin/users/{user.Id}");
                Users.Remove(user);
                CountText = $"{Users.Count} user{(Users.Count == 1 ? "" : "s")}";
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Delete failed", ex.Message, "OK");
            }
        }

        // ============================================================
        // BACK
        // ============================================================
        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("//main");
        }
    }
}