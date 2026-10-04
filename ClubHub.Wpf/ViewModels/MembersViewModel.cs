using System.Collections.ObjectModel;
using ClubHub.Core.Interfaces;
using ClubHub.Core.Models;

namespace ClubHub.Wpf.ViewModels;

public class MembersViewModel : PageViewModel
{
    public override string Title => "Members";

    public ObservableCollection<Member> Members { get; }

    public MembersViewModel(IRepository<Member> memberRepository)
    {
        Members = new ObservableCollection<Member>(memberRepository.GetAll().OrderBy(m => m.LastName));
    }
}
